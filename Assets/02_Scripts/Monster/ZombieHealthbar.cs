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

    private void LateUpdate()
    {
        _slider.value = _zombie.CurrentHealthRatio;
        transform.forward = Camera.main.transform.forward;
    }
}
