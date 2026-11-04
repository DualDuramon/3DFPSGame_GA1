using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로그인 패널의 입력 처리, 로그인/회원가입 화면 전환, 씬 전환을 담당한다
// 아직 서버 인증은 없으므로 ID/암호가 비어있지 않으면 통과시킨다
public class UI_Login : MonoBehaviour
{
    private enum LoginMode
    {
        Login,
        Register
    }

    private const string LoginHeader = "SURVIVOR LOGIN";
    private const string RegisterHeader = "SURVIVOR REGISTER";

    [Header("입력 칸")]
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passWordInput;
    [SerializeField] private TMP_InputField _passWordConfirmInput;

    [Header("버튼")]
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _gotoRegisterButton;
    [SerializeField] private Button _registerButton;
    [SerializeField] private Button _gotoLoginButton;
    [SerializeField] private Button _recoverButton;

    [Header("메시지")]
    [SerializeField] private TextMeshProUGUI _headerText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Color _normalColor = new Color(0.55f, 1f, 0.71f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.35f, 0.29f);

    [Header("모드 전환 연출")]
    [SerializeField] private CanvasGroup _loginGroup;
    [SerializeField] private CanvasGroup _registerGroup;
    [SerializeField] private float _loginPanelHeight = 600f;
    [SerializeField] private float _registerPanelHeight = 720f;
    [SerializeField] private float _transitionDuration = 0.35f;
    [SerializeField] private float _slideOffset = 20f;

    [Header("팝업")]
    [SerializeField] private UI_Popup _warningPopup;
    [SerializeField] private UI_Popup _successPopup;

    [Header("씬 전환")]
    [SerializeField] private string _nextSceneName = "GameScene";
    [SerializeField] private float _loadDelay = 1.2f;

    private RectTransform _panel;
    private LoginMode _mode;
    private bool _isLoading;
    private bool _isTransitioning;
    private TMP_InputField _focusAfterWarning;

    private bool IsPopupOpen => _warningPopup.IsOpen || _successPopup.IsOpen;

    private void Awake()
    {
        _panel = (RectTransform)transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _loginButton.onClick.AddListener(Login);
        _gotoRegisterButton.onClick.AddListener(() => SetMode(LoginMode.Register));
        _registerButton.onClick.AddListener(Register);
        _gotoLoginButton.onClick.AddListener(() => SetMode(LoginMode.Login));
        _recoverButton.onClick.AddListener(Recover);

        // Enter: ID → 암호 → (회원가입이면 암호 확인 →) 제출
        _idInput.onSubmit.AddListener(_ => _passWordInput.ActivateInputField());
        _passWordInput.onSubmit.AddListener(_ => OnSubmitPassword());
        _passWordConfirmInput.onSubmit.AddListener(_ => Register());

        _warningPopup.OnConfirm += () => _focusAfterWarning.ActivateInputField();
        _successPopup.OnConfirm += () => SetMode(LoginMode.Login);

        _warningPopup.gameObject.SetActive(false);
        _successPopup.gameObject.SetActive(false);
        ApplyModeImmediate(LoginMode.Login);
    }

    private void Update()
    {
        if (_isLoading || _isTransitioning || IsPopupOpen) return;

        // Tab으로 입력칸 순환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetNextInput().ActivateInputField();
        }
    }

    public void Login()
    {
        if (_isLoading || _isTransitioning) return;

        if (!TryValidateIdAndPassword(out string error, out TMP_InputField errorInput))
        {
            ShowMessage($"> 오류: {error}", _errorColor);
            errorInput.ActivateInputField();
            return;
        }

        StartCoroutine(Login_Coroutine());
    }

    public void Register()
    {
        if (_isLoading || _isTransitioning || IsPopupOpen) return;

        if (!TryValidateIdAndPassword(out string error, out TMP_InputField errorInput))
        {
            ShowWarning(error, errorInput);
            return;
        }

        if (_passWordInput.text != _passWordConfirmInput.text)
        {
            _passWordConfirmInput.text = string.Empty;
            ShowWarning("암호가 일치하지 않습니다.", _passWordConfirmInput);
            return;
        }

        // TODO: 서버가 생기면 실제 계정 생성 요청으로 교체 -> ID 중복 검사, 실제 회원가입 데이터 저장
        GameLogger.Log($"회원가입: {_idInput.text}");
        _successPopup.Open();
    }

    public void Recover()
    {
        ShowMessage("> 암호 복구 기능은 준비 중입니다.", _normalColor);
    }

    private void SetMode(LoginMode mode)
    {
        if (_isTransitioning || _mode == mode) return;

        StartCoroutine(Transition_Coroutine(mode));
    }

    private void ApplyModeImmediate(LoginMode mode)
    {
        _mode = mode;
        bool isLogin = mode == LoginMode.Login;

        SetGroupVisible(_loginGroup, isLogin);
        SetGroupVisible(_registerGroup, !isLogin);
        SetPanelHeight(isLogin ? _loginPanelHeight : _registerPanelHeight);
        _headerText.text = isLogin ? LoginHeader : RegisterHeader;

        ResetInputs();
        _idInput.ActivateInputField();
    }

    // 이전 그룹을 페이드아웃하는 동안 패널 높이를 바꾸고, 후반부에 다음 그룹을 아래에서 밀어 올리며 페이드인한다
    private IEnumerator Transition_Coroutine(LoginMode mode)
    {
        _isTransitioning = true;
        _mode = mode;
        bool toRegister = mode == LoginMode.Register;

        CanvasGroup from = toRegister ? _loginGroup : _registerGroup;
        CanvasGroup to = toRegister ? _registerGroup : _loginGroup;
        RectTransform toRect = (RectTransform)to.transform;
        float fromHeight = _panel.sizeDelta.y;
        float toHeight = toRegister ? _registerPanelHeight : _loginPanelHeight;

        ResetInputs();
        from.interactable = false;
        to.interactable = false;
        to.alpha = 0f;
        to.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime / _transitionDuration);
            SetPanelHeight(Mathf.Lerp(fromHeight, toHeight, Mathf.SmoothStep(0f, 1f, t)));

            from.alpha = 1f - Mathf.Clamp01(t * 2f);

            float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2f - 1f));
            to.alpha = fadeIn;
            toRect.anchoredPosition = new Vector2(0f, -_slideOffset * (1f - fadeIn));

            if (t >= 0.5f)
            {
                _headerText.text = toRegister ? RegisterHeader : LoginHeader;
            }

            yield return null;
        }

        SetGroupVisible(from, false);
        SetGroupVisible(to, true);
        _isTransitioning = false;

        // 가입 후 돌아왔으면 ID는 남아 있으니 암호칸부터 입력
        TMP_InputField focus = string.IsNullOrEmpty(_idInput.text) ? _idInput : _passWordInput;
        focus.ActivateInputField();
    }

    private void SetGroupVisible(CanvasGroup group, bool isVisible)
    {
        group.gameObject.SetActive(isVisible);
        group.alpha = isVisible ? 1f : 0f;
        group.interactable = isVisible;
        ((RectTransform)group.transform).anchoredPosition = Vector2.zero;
    }

    private void SetPanelHeight(float height)
    {
        _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, height);
    }

    private void ResetInputs()
    {
        _passWordInput.text = string.Empty;
        _passWordConfirmInput.text = string.Empty;
        _messageText.text = string.Empty;
    }

    private bool TryValidateIdAndPassword(out string error, out TMP_InputField errorInput)
    {
        if (string.IsNullOrWhiteSpace(_idInput.text))
        {
            error = "생존자 ID를 입력하십시오.";
            errorInput = _idInput;
            return false;
        }

        if (string.IsNullOrEmpty(_passWordInput.text))
        {
            error = "암호를 입력하십시오.";
            errorInput = _passWordInput;
            return false;
        }

        //TODO : ID 및 비밀번호 규칙 -> 막 비번 7~8글자 이상이니 id에 특수문자 못 넣는다느니 등등

        error = null;
        errorInput = null;
        return true;
    }

    private void ShowWarning(string message, TMP_InputField focusAfter)
    {
        _focusAfterWarning = focusAfter;
        _warningPopup.Open(message);
    }

    private void OnSubmitPassword()
    {
        if (_mode == LoginMode.Login)
        {
            Login();
        }
        else
        {
            _passWordConfirmInput.ActivateInputField();
        }
    }

    private TMP_InputField GetNextInput()
    {
        if (_idInput.isFocused) return _passWordInput;
        if (_passWordInput.isFocused && _mode == LoginMode.Register) return _passWordConfirmInput;

        return _idInput;
    }

    private IEnumerator Login_Coroutine()
    {
        _isLoading = true;
        _loginButton.interactable = false;
        _idInput.interactable = false;
        _passWordInput.interactable = false;

        ShowMessage($"> 신원 확인 완료. {_idInput.text}님, 행운을 빕니다.", _normalColor);

        yield return new WaitForSeconds(_loadDelay);
        SceneManager.LoadScene(_nextSceneName);
    }

    private void ShowMessage(string message, Color color)
    {
        _messageText.text = message;
        _messageText.color = color;
    }
}
