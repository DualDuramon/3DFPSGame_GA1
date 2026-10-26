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

    private void OnEnable()
    {
        _playerStatus.OnTakeHit += UpdateHealthBar;
    }

    private void OnDisable()
    {
        _playerStatus.OnTakeHit -= UpdateHealthBar;
    }

    private void Update()
    {
        if (_playerStatus == null) return;

        //문제 : Update에서 계속 체력갱신 연산을 해버리는건 불필요한 연산이다.
        //특히 이런게 많아질수록 CPU가 쉬질 못한다 -> 쓰로틀링이 걸려 렉걸린다
        //UpdateHealthBar() //폴링 방식

        if (_staminaSlider != null)
        {
            _staminaSlider.minValue = 0f;
            _staminaSlider.maxValue = 1f;
            _staminaSlider.value = _playerStatus.CurrentStaminaRatio;
        }
    }

    private void UpdateHealthBar()
    {
        if (_healthSlider != null)
        {
            _healthSlider.minValue = 0f;
            _healthSlider.maxValue = 1f;
            _healthSlider.value = _playerStatus.CurrentHealthRatio;
        }
    }

}
