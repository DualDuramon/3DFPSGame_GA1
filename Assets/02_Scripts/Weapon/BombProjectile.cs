using Lean.Pool;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무언가에 부딪히면 폭발 이펙트를 남기고 사라지는 bomb
/// </summary>
public class BombProjectile : MonoBehaviour, IPoolable
{
    [Header("References")]
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private Rigidbody _rigid;
    [Header("Damage Settings")]
    [SerializeField] private float _explosionDamage = 50f;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private LayerMask _damageableLayer;


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


        SpreadExplosionDamage();

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

    private void SpreadExplosionDamage()
    {
        Vector3 explosionPosition = transform.position;

        Collider[] hits = Physics.OverlapSphere(explosionPosition, _explosionRadius, _damageableLayer, QueryTriggerInteraction.Ignore);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, this) || !damagedTargets.Add(target)) continue;

            Vector3 hitPoint = hit.ClosestPoint(explosionPosition);
            Vector3 hitNormal = explosionPosition - hitPoint;
            if (hitNormal.sqrMagnitude < 0.0001f)
            {
                hitNormal = explosionPosition - hit.bounds.center;
            }
            hitNormal = hitNormal.sqrMagnitude > 0.0001f ? hitNormal.normalized : Vector3.up;

            target.TakeDamage(new DamageInfo(_explosionDamage, hitPoint, hitNormal));
        }
    }
}
