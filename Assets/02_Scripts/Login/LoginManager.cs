using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로그인 씬의 입력 처리와 씬 전환을 담당한다
// 아직 서버 인증은 없으므로 ID/암호가 비어있지 않으면 통과시킨다
public class LoginManager : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private Button _loginButton;

    [Header("메시지")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Color _normalColor = new Color(0.55f, 1f, 0.71f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.35f, 0.29f);

    [Header("씬 전환")]
    [SerializeField] private string _nextSceneName = "GameScene";
    [SerializeField] private float _loadDelay = 1.2f;

    private bool _isLoading;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _loginButton.onClick.AddListener(OnClickLogin);
        _idInput.onSubmit.AddListener(_ => _passwordInput.ActivateInputField());
        _passwordInput.onSubmit.AddListener(_ => OnClickLogin());

        _messageText.text = string.Empty;
        _idInput.ActivateInputField();
    }

    private void Update()
    {
        if (_isLoading) return;

        // Tab으로 ID ↔ 암호 입력칸 이동
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TMP_InputField next = _idInput.isFocused ? _passwordInput : _idInput;
            next.ActivateInputField();
        }
    }

    public void OnClickLogin()
    {
        if (_isLoading) return;

        if (string.IsNullOrWhiteSpace(_idInput.text))
        {
            ShowMessage("> 오류: 생존자 ID를 입력하십시오.", _errorColor);
            _idInput.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(_passwordInput.text))
        {
            ShowMessage("> 오류: 암호를 입력하십시오.", _errorColor);
            _passwordInput.ActivateInputField();
            return;
        }

        StartCoroutine(Login_Coroutine());
    }

    public void OnClickRegister()
    {
        ShowMessage("> 신규 등록 창구는 아직 열리지 않았습니다.", _normalColor);
    }

    public void OnClickRecover()
    {
        ShowMessage("> 암호 복구 기능은 준비 중입니다.", _normalColor);
    }

    private IEnumerator Login_Coroutine()
    {
        _isLoading = true;
        _loginButton.interactable = false;
        _idInput.interactable = false;
        _passwordInput.interactable = false;

        GameLogger.Log($"로그인: {_idInput.text}");
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
