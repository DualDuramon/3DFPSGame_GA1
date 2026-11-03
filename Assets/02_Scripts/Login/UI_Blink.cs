using UnityEngine;
using UnityEngine.UI;

// Graphic을 일정 간격으로 켰다 껐다 한다 (터미널 커서, REC 표시 등)
[RequireComponent(typeof(Graphic))]
public class UI_Blink : MonoBehaviour
{
    [SerializeField] private float _interval = 0.5f;

    private Graphic _graphic;
    private float _timer;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer < _interval) return;

        _timer = 0f;
        _graphic.enabled = !_graphic.enabled;
    }
}
