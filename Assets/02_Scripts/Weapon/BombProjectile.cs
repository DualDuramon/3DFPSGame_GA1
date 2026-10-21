using Lean.Pool;
using UnityEngine;

/// <summary>
/// 무언가에 부딪히면 폭발 이펙트를 남기고 사라지는 bomb
/// </summary>
public class BombProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private Rigidbody _rigid;
    private bool _hasExploded;

    private void Awake()
    {
        if (_rigid == null)
        {
            _rigid = GetComponent<Rigidbody>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        if (_explosionEffectPrefab != null)
        {
            LeanPool.Spawn(_explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        LeanPool.Despawn(gameObject);
    }

    public void OnSpawn()
    {
        _hasExploded = false;
        ResetMotion();
    }

    public void OnDespawn()
    {
        _hasExploded = true;
        ResetMotion();
    }

    private void ResetMotion()
    {
        _rigid.linearVelocity = Vector3.zero;
        _rigid.angularVelocity = Vector3.zero;
    }

    public void Throw(Vector3 throwVector)
    {
        if (throwVector == Vector3.zero) return;

        _rigid.AddForce(throwVector, ForceMode.Impulse);
    }
}
