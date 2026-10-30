using UnityEngine;

public class SearchLight : MonoBehaviour, IDamageable
{
    [SerializeField] private Light _lightSource;
    public void TakeDamage(DamageInfo damageInfo)
    {
        _lightSource.gameObject.SetActive(false);
    }

    private void Awake()
    {
        _lightSource = GetComponentInChildren<Light>();
    }
}
