using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerMove _target;
    [SerializeField] private Slider _slider;

    private void Awake()
    {
        if (_target == null)
        {
            _target = FindAnyObjectByType<PlayerMove>();
        }

        if (_slider == null)
        {
            _slider = GetComponent<Slider>();
        }
    }

    private void Update()
    {
        _slider.value = _target.CurrentStaminaRatio;
    }
}
