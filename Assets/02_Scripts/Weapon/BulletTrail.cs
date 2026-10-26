using System.Collections;
using UnityEngine;

/// <summary>
/// 총구에서 적중 지점까지의 총알 궤적을 직선으로 그리는 LineRenderer 래퍼.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BulletTrail : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color _color = Color.yellow;

    [Header("Playback")]
    [SerializeField] private float _duration = 0.05f;

    private LineRenderer _lineRenderer;
    private Coroutine _hideCoroutine;

    public Color Color => _color;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;
        ApplyColor();
    }

    public void SetColor(Color color)
    {
        _color = color;
        ApplyColor();
    }

    /// <summary>
    /// start에서 end까지 직선 궤적을 표시한다.
    /// </summary>
    public void Draw(Vector3 start, Vector3 end)
    {
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
        _lineRenderer.enabled = true;
    }

    /// <summary>
    /// 궤적을 그린 뒤 _duration초 후에 자동으로 숨긴다. 재생 중에 다시 호출하면 시간이 새로 시작된다.
    /// </summary>
    public void Play(Vector3 start, Vector3 end)
    {
        Draw(start, end);

        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }
        _hideCoroutine = StartCoroutine(HideAfterDuration_Coroutine());
    }

    public void Hide()
    {
        _lineRenderer.enabled = false;
    }

    private IEnumerator HideAfterDuration_Coroutine()
    {
        yield return new WaitForSeconds(_duration);

        Hide();
        _hideCoroutine = null;
    }

    private void ApplyColor()
    {
        _lineRenderer.startColor = _color;
        _lineRenderer.endColor = _color;
    }
}
