using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerTakeDamageEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCommonStatus _targetStatus;
    [SerializeField] private Image _vignetteImage;
    [SerializeField] AnimationCurve _vignettedCurve;
    [SerializeField] private float _vignetteTime = 1f;

    private Color _startColor;
    private Coroutine _vignetteCoroutine;

    private void Awake()
    {
        if (_targetStatus == null)
        {
            _targetStatus = FindAnyObjectByType<PlayerCommonStatus>();
        }

        if (_vignetteImage == null)
        {
            _vignetteImage = GetComponentInChildren<Image>();
        }
        _startColor = _vignetteImage.color;
        SetColorTransparency();
    }

    private void OnEnable()
    {
        _targetStatus.OnTakeHit += StartDamageVigenette;
    }

    private void OnDisable()
    {
        _targetStatus.OnTakeHit -= StartDamageVigenette;
    }

    private void StartDamageVigenette()
    {
        if (_vignetteCoroutine != null)
        {
            StopCoroutine(_vignetteCoroutine);
            SetColorTransparency();
        }

        _vignetteCoroutine = StartCoroutine(DamageVignetteCoroutine());
    }

    private IEnumerator DamageVignetteCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < _vignetteTime)
        {
            elapsed += Time.deltaTime;
            float alpha = _startColor.a * _vignettedCurve.Evaluate(elapsed / _vignetteTime);
            Color newColor = _vignetteImage.color;
            newColor.a = alpha;

            _vignetteImage.color = newColor;

            yield return null;
        }

        _vignetteImage.color = _startColor;
    }

    private void SetColorTransparency()
    {
        Color tempColor = _vignetteImage.color;
        tempColor.a = 0f;

        _vignetteImage.color = tempColor;
    }


}
