using UnityEngine;

public class Box : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 30f;
    [SerializeField] private GameObject _destroyedPrefab;

    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        _health -= damage;

        if (damage <= 0)
        {
            HandleDeath();
            return;
        }
    }

    private void HandleDeath()
    {
        //Instantiate(_destroyedPrefab);
        Destroy(gameObject);
    }
}
