using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthbar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Zombie _zombie;

    private void Awake()
    {
        _slider = GetComponentInChildren<Slider>();
        _zombie = GetComponentInParent<Zombie>();
    }

    private void OnEnable()
    {
        _zombie.OnTakeHit += UpdateHealthBar;
    }

    private void OnDisable()
    {
        _zombie.OnTakeHit -= UpdateHealthBar;
    }

    private void LateUpdate()
    {
        _slider.transform.forward = Camera.main.transform.forward;
    }

    private void UpdateHealthBar()
    {
        _slider.value = _zombie.CurrentHealthRatio;
    }
}
