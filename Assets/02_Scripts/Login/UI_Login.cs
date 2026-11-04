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
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Color _normalColor = new Color(0.55f, 1f, 0.71f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.35f, 0.29f);

    [Header("씬 전환")]
    [SerializeField] private string _nextSceneName = "GameScene";
    [SerializeField] private float _loadDelay = 1.2f;

    private LoginMode _mode;
    private bool _isLoading;

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

        SetMode(LoginMode.Login);
    }

    private void Update()
    {
        if (_isLoading) return;

        // Tab으로 입력칸 순환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetNextInput().ActivateInputField();
        }
    }

    public void Login()
    {
        if (_isLoading) return;
        if (!ValidateIdAndPassword()) return;

        StartCoroutine(Login_Coroutine());
    }

    public void Register()
    {
        if (_isLoading) return;
        if (!ValidateIdAndPassword()) return;

        if (_passWordInput.text != _passWordConfirmInput.text)
        {
            ShowMessage("> 오류: 암호가 일치하지 않습니다.", _errorColor);
            _passWordConfirmInput.text = string.Empty;
            _passWordConfirmInput.ActivateInputField();
            return;
        }

        // TODO: 서버가 생기면 실제 계정 생성 요청으로 교체 -> ID 중복 검사, 실제 회원가입 데이터 저장
        GameLogger.Log($"회원가입: {_idInput.text}");
        SetMode(LoginMode.Login);

        ShowMessage($"> 생존자 {_idInput.text} 등록 완료. 로그인하십시오.", _normalColor);
        _passWordInput.ActivateInputField();
    }

    public void Recover()
    {
        ShowMessage("> 암호 복구 기능은 준비 중입니다.", _normalColor);
    }

    private void SetMode(LoginMode mode)
    {
        _mode = mode;
        bool isLogin = mode == LoginMode.Login;

        _loginButton.gameObject.SetActive(isLogin);
        _gotoRegisterButton.gameObject.SetActive(isLogin);
        _recoverButton.gameObject.SetActive(isLogin);

        _passWordConfirmInput.gameObject.SetActive(!isLogin);
        _registerButton.gameObject.SetActive(!isLogin);
        _gotoLoginButton.gameObject.SetActive(!isLogin);

        _passWordInput.text = string.Empty;
        _passWordConfirmInput.text = string.Empty;
        _messageText.text = string.Empty;
        _idInput.ActivateInputField();
    }

    private bool ValidateIdAndPassword()
    {
        if (string.IsNullOrWhiteSpace(_idInput.text))
        {
            ShowMessage("> 오류: 생존자 ID를 입력하십시오.", _errorColor);
            _idInput.ActivateInputField();
            return false;
        }

        if (string.IsNullOrEmpty(_passWordInput.text))
        {
            ShowMessage("> 오류: 암호를 입력하십시오.", _errorColor);
            _passWordInput.ActivateInputField();
            return false;
        }

        //TODO : ID 및 비밀번호 규칙 -> 막 비번 7~8글자 이상이니 id에 특수문자 못 넣는다느니 등등

        return true;
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
