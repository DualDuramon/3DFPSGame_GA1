using UnityEngine;

public class Box : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 30f;
    [SerializeField] private GameObject _destroyedPrefab;

    public void TakeDamage(DamageInfo info)
    {
        if (info.DamageValue <= 0) return;

        _health -= info.DamageValue;

        if (_health <= 0)
        {
            HandleDeath();
            return;
        }
    }

    private void HandleDeath()
    {
        GameObject obj = Instantiate(_destroyedPrefab, transform.position, Quaternion.identity);

        //TODO: 이건 상자 잔해 오브젝트에게..
        Rigidbody[] bodies = obj.GetComponentsInChildren<Rigidbody>();

        Vector3[] directions =
        {
            Vector3.up,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        foreach (Rigidbody b in bodies)
        {
            Vector3 direction = directions[Random.Range(0, directions.Length)];
            b.AddForce(direction * 2f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }
}
