using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 확인 버튼 하나짜리 팝업. 열릴 때 페이드인과 함께 박스가 살짝 커진다
[RequireComponent(typeof(CanvasGroup))]
public class UI_Popup : MonoBehaviour
{
    [SerializeField] private RectTransform _box;
    [SerializeField] private TextMeshProUGUI _messageText; // 비워두면 씬에 적힌 문구를 그대로 쓴다
    [SerializeField] private Button _confirmButton;

    [Header("연출")]
    [SerializeField] private float _openDuration = 0.18f;
    [SerializeField] private float _startScale = 0.9f;

    private CanvasGroup _canvasGroup;
    private int _openedFrame;

    public event Action OnConfirm;
    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _confirmButton.onClick.AddListener(Confirm);
    }

    private void Update()
    {
        // 팝업을 연 Enter 입력이 같은 프레임에 바로 닫지 않도록 한 프레임 건너뛴다
        if (Time.frameCount == _openedFrame) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Confirm();
        }
    }

    public void Open(string message = null)
    {
        if (_messageText != null && message != null)
        {
            _messageText.text = message;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        _openedFrame = Time.frameCount;

        // 입력칸 포커스를 풀어 팝업 뒤에서 타이핑되지 않게 한다
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        StartCoroutine(Open_Coroutine());
    }

    private void Confirm()
    {
        gameObject.SetActive(false);
        OnConfirm?.Invoke();
    }

    private IEnumerator Open_Coroutine()
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(1f, t + Time.unscaledDeltaTime / _openDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            _canvasGroup.alpha = eased;
            _box.localScale = Vector3.one * Mathf.Lerp(_startScale, 1f, eased);
            yield return null;
        }
    }
}
