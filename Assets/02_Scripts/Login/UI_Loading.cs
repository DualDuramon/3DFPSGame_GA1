using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// LoadingScene의 UI를 담당한다
// SceneLoader.NextSceneName 씬을 비동기로 불러오면서 진행률, 상태 메시지, 팁, 연출을 갱신하고
// 로드가 끝나면(최소 표시 시간 이후) 다음 씬을 활성화한다
public class UI_Loading : MonoBehaviour
{
    // allowSceneActivation = false 이면 progress는 0.9에서 멈춘다
    private const float LoadedProgress = 0.9f;

    [Header("씬 전환")]
    [Tooltip("LoadingScene을 직접 실행해서 SceneLoader에 다음 씬이 없을 때 사용할 씬")]
    [SerializeField] private string _fallbackSceneName = "GameScene";
    [Tooltip("로드가 빨리 끝나도 로딩 화면을 최소 이 시간만큼 보여준다")]
    [SerializeField] private float _minLoadingTime = 3f;
    [Tooltip("100%를 보여준 뒤 씬을 전환하기까지 기다리는 시간")]
    [SerializeField] private float _completeHoldTime = 0.5f;
    [Tooltip("표시용 진행률이 실제 진행률을 따라가는 속도 (초당)")]
    [SerializeField] private float _fillSpeed = 0.8f;

    [Header("진행률")]
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _percentText;

    [Header("상태 메시지")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [Tooltip("진행률 구간마다 순서대로 보여줄 메시지 (마지막 메시지는 100%일 때)")]
    [SerializeField]
    private string[] _statusMessages =
    {
        "> 격리 게이트 잠금 해제 중",
        "> 구역 전력 복구 중",
        "> 감염체 반응 신호 추적 중",
        "> 무기 상태 점검 중",
        "> 게이트 개방 완료."
    };
    [SerializeField] private float _dotInterval = 0.4f;

    [Header("부제")]
    [SerializeField] private TextMeshProUGUI _subTitleText;
    [SerializeField] private string _subTitleLoading = "QUARANTINE GATE OPENING";
    [SerializeField] private string _subTitleComplete = "QUARANTINE GATE OPEN";
    [SerializeField] private float _subTitlePulseSpeed = 2.5f;
    [SerializeField] private float _subTitleMinAlpha = 0.35f;

    [Header("팁")]
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private string _tipPrefix = "TIP  ·  ";
    [SerializeField]
    private string[] _tips =
    {
        "수류탄은 좀비 무리 한가운데에 던지세요.",
        "탄창이 비기 전에 미리 재장전하세요.",
        "좀비는 머리를 노리면 더 빨리 쓰러집니다.",
        "달리기는 스태미나를 소모합니다. 도망칠 힘은 남겨두세요.",
        "드럼통을 쏘면 주변의 좀비를 한꺼번에 날려버릴 수 있습니다."
    };
    [SerializeField] private float _tipInterval = 2.5f;

    [Header("스캔라인")]
    [SerializeField] private Graphic _scanline;
    [SerializeField] private float _scanlineFlickerInterval = 0.08f;
    [SerializeField] private float _scanlineMinAlpha = 0.03f;
    [SerializeField] private float _scanlineMaxAlpha = 0.08f;

    private float _displayProgress;
    private bool _isComplete;

    private int _statusIndex = -1;
    private float _dotTimer;
    private int _dotCount;

    private int _tipIndex = -1;
    private float _tipTimer;

    private float _scanlineTimer;

    private void Start()
    {
        SetProgress(0f);
        ShowNextTip();
        StartCoroutine(LoadCoroutine());
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        UpdateStatus(dt);
        UpdateSubTitle();
        UpdateTip(dt);
        UpdateScanline(dt);
    }

    private IEnumerator LoadCoroutine()
    {
        string sceneName = string.IsNullOrEmpty(SceneLoader.NextSceneName)
            ? _fallbackSceneName
            : SceneLoader.NextSceneName;

        // 비동기 씬 로드, 진행 상황을 확인할 수 있는 객체를 반환한다
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            GameLogger.LogError($"씬을 불러올 수 없습니다: {sceneName} (Build Settings 등록 여부를 확인하세요)");
            yield break;
        }

        // 로드가 끝나도 바로 전환하지 않고 0.9에서 대기시킨다
        operation.allowSceneActivation = false;

        float elapsed = 0f;
        while (_displayProgress < 1f)
        {
            elapsed += Time.unscaledDeltaTime;

            // 실제 진행률(0~0.9 → 0~1)과 최소 표시 시간 중 느린 쪽을 목표로 삼는다
            float loadRatio = Mathf.Clamp01(operation.progress / LoadedProgress);
            float timeRatio = _minLoadingTime > 0f ? Mathf.Clamp01(elapsed / _minLoadingTime) : 1f;
            float target = Mathf.Min(loadRatio, timeRatio);

            SetProgress(Mathf.MoveTowards(_displayProgress, target, _fillSpeed * Time.unscaledDeltaTime));

            yield return null;
        }

        _isComplete = true;
        GameLogger.Log($"{sceneName} 로드 완료");

        yield return new WaitForSecondsRealtime(_completeHoldTime);

        // 씬 활성화: 이 시점에 다음 씬의 Awake/Start가 실행되고 LoadingScene은 언로드된다
        operation.allowSceneActivation = true;
    }

    private void SetProgress(float value)
    {
        _displayProgress = value;
        _progressSlider.value = value;
        _percentText.text = Mathf.FloorToInt(value * 100f).ToString();
    }

    // 진행률 구간에 맞는 메시지 + 깜빡이는 말줄임표
    private void UpdateStatus(float dt)
    {
        int last = _statusMessages.Length - 1;
        int index = _isComplete ? last : Mathf.Min(Mathf.FloorToInt(_displayProgress * last), last - 1);

        if (index != _statusIndex)
        {
            _statusIndex = index;
            _dotCount = 0;
            _dotTimer = 0f;
        }

        if (_isComplete)
        {
            _statusText.text = _statusMessages[index];
            return;
        }

        _dotTimer += dt;
        if (_dotTimer >= _dotInterval)
        {
            _dotTimer = 0f;
            _dotCount = (_dotCount + 1) % 4;
        }

        _statusText.text = _statusMessages[index] + new string('.', _dotCount);
    }

    // 로딩 중에는 부제가 숨쉬듯 깜빡이고, 완료되면 문구가 바뀌며 고정된다
    private void UpdateSubTitle()
    {
        Color color = _subTitleText.color;

        if (_isComplete)
        {
            _subTitleText.text = _subTitleComplete;
            color.a = 1f;
        }
        else
        {
            _subTitleText.text = _subTitleLoading;
            float wave = (Mathf.Sin(Time.unscaledTime * _subTitlePulseSpeed) + 1f) * 0.5f;
            color.a = Mathf.Lerp(_subTitleMinAlpha, 1f, wave);
        }

        _subTitleText.color = color;
    }

    private void UpdateTip(float dt)
    {
        _tipTimer += dt;
        if (_tipTimer < _tipInterval) return;

        _tipTimer = 0f;
        ShowNextTip();
    }

    // 직전과 다른 팁을 무작위로 고른다
    private void ShowNextTip()
    {
        if (_tips.Length == 0) return;

        int index = Random.Range(0, _tips.Length);
        if (_tips.Length > 1 && index == _tipIndex)
        {
            index = (index + 1) % _tips.Length;
        }

        _tipIndex = index;
        _tipText.text = _tipPrefix + _tips[index];
    }

    // CRT 화면처럼 스캔라인 밝기를 불규칙하게 흔든다
    private void UpdateScanline(float dt)
    {
        _scanlineTimer += dt;
        if (_scanlineTimer < _scanlineFlickerInterval) return;

        _scanlineTimer = 0f;

        Color color = _scanline.color;
        color.a = Random.Range(_scanlineMinAlpha, _scanlineMaxAlpha);
        _scanline.color = color;
    }
}
