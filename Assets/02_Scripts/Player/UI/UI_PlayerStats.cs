using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    [SerializeField] private PlayerCommonStatus _playerStatus;

    private void Awake()
    {
        if (_playerStatus == null)
        {
            _playerStatus = FindAnyObjectByType<PlayerCommonStatus>();
        }
        if (_healthSlider == null && transform.childCount > 0)
        {
            _healthSlider = transform.GetChild(0).GetComponent<Slider>();
        }
        if (_staminaSlider == null && transform.childCount > 1)
        {
            _staminaSlider = transform.GetChild(1).GetComponent<Slider>();
        }
    }

    private void LateUpdate()
    {
        if (_playerStatus == null) return;

        if (_healthSlider != null)
        {
            _healthSlider.minValue = 0f;
            _healthSlider.maxValue = 1f;
            _healthSlider.value = _playerStatus.CurrentHealthRatio;
        }
        if (_staminaSlider != null)
        {
            _staminaSlider.minValue = 0f;
            _staminaSlider.maxValue = 1f;
            _staminaSlider.value = _playerStatus.CurrentStaminaRatio;
        }
    }
}
