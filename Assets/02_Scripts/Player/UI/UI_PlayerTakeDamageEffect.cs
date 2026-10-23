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
        float elapsedTime = 0f;
        _vignetteImage.color = _startColor;

        while (elapsedTime < _vignetteTime)
        {
            elapsedTime += Time.deltaTime;

            float alpha = _startColor.a * _vignettedCurve.Evaluate(elapsedTime / _vignetteTime);
            SetAlpha(alpha);

            yield return null;
        }
        SetColorTransparency();
    }

    private void SetColorTransparency()
    {
        SetAlpha(0f);
    }

    private void SetAlpha(float newAlpha)
    {
        Color newColor = _vignetteImage.color;
        newColor.a = newAlpha;

        _vignetteImage.color = newColor;
    }
}
