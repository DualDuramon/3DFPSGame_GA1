using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPostProcessingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume _nightVisionVolume;
    [SerializeField] private Light _nightVisionLight;

    [Header("Night Vision Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.T;
    [SerializeField] private float _fadeSpeed = 8f;

    private bool _isNightVisionOn = false;

    public bool IsNightVisionOn => _isNightVisionOn;


    private void Awake()
    {
        if (_nightVisionVolume == null)
        {
            GameLogger.LogWarning($"{name}: Night Vision Volume Doesn't Find!");
            enabled = false;
            return;
        }

        _nightVisionVolume.weight = 0f;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        if (Input.GetKeyDown(_toggleKey))
        {
            SetNightVision(!_isNightVisionOn);
        }

        // 켜고 끌 때 Volume 가중치를 부드럽게 전환한다.
        float targetWeight = _isNightVisionOn ? 1f : 0f;
        _nightVisionVolume.weight = Mathf.MoveTowards(_nightVisionVolume.weight, targetWeight, _fadeSpeed * Time.deltaTime);
    }

    public void SetNightVision(bool isOn)
    {
        _isNightVisionOn = isOn;

        _nightVisionLight.gameObject.SetActive(isOn);
    }
}
