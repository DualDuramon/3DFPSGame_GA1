using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// CRT 모니터 연출: 화면 떨림, 위에서 아래로 흐르는 밝은 띠, CCTV 노이즈, 시계
public class UI_CrtScreen : MonoBehaviour
{
    [Header("화면 떨림")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private float _jitterInterval = 4f;
    [SerializeField] private float _jitterDuration = 0.12f;
    [SerializeField] private float _jitterAmount = 6f;

    [Header("롤링 바")]
    [SerializeField] private RectTransform _rollBar;
    [SerializeField] private float _rollDuration = 7f;

    [Header("CCTV 피드")]
    [SerializeField] private Graphic _feed;
    [SerializeField] private float _feedNoiseInterval = 0.075f;
    [SerializeField] private float _feedMinAlpha = 0.8f;
    [SerializeField] private TextMeshProUGUI _clockText;

    private RectTransform _screen;
    private float _jitterTimer;
    private float _rollTimer;
    private float _feedTimer;

    private void Awake()
    {
        _screen = (RectTransform)transform;
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        UpdateJitter(dt);
        UpdateRollBar(dt);
        UpdateFeed(dt);
    }

    private void UpdateJitter(float dt)
    {
        _jitterTimer += dt;

        bool isJittering = _jitterTimer > _jitterInterval - _jitterDuration;
        float offset = isJittering ? UnityEngine.Random.Range(-_jitterAmount, _jitterAmount) : 0f;
        _content.anchoredPosition = new Vector2(offset, 0f);

        if (_jitterTimer >= _jitterInterval)
        {
            _jitterTimer = 0f;
        }
    }

    private void UpdateRollBar(float dt)
    {
        _rollTimer = (_rollTimer + dt) % _rollDuration;

        // 화면 위쪽 바깥에서 시작해 아래쪽 바깥까지 이동
        float height = _screen.rect.height + _rollBar.rect.height;
        float y = _rollBar.rect.height - height * (_rollTimer / _rollDuration);
        _rollBar.anchoredPosition = new Vector2(0f, y);
    }

    private void UpdateFeed(float dt)
    {
        _feedTimer += dt;
        if (_feedTimer < _feedNoiseInterval) return;

        _feedTimer = 0f;

        Color color = _feed.color;
        color.a = UnityEngine.Random.Range(_feedMinAlpha, 1f);
        _feed.color = color;

        _clockText.text = DateTime.Now.ToString("HH:mm:ss");
    }
}
