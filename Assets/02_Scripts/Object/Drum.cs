using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 체력이 0이 되면 위로 튀어오르며 폭발하는 드럼통
[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour, IDamageable
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private LayerMask _damageableMask;

    [Header("스탯")]
    [SerializeField] private float _health = 30f;
    [SerializeField, Min(0f)] private float _explosionDamage = 30f;
    [SerializeField, Min(0f)] private float _explosionRadius = 5f;
    [SerializeField] private float _launchPower = 20f;
    [SerializeField] private float _spinPower = 5f;
    [SerializeField] private float _destoryDelay = 3f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void TakeDamage(DamageInfo info)
    {
        if (_health <= 0)
        {
            return;
        }

        _health -= info.DamageValue;

        if (_health <= 0)
        {
            StartCoroutine(Explode_Coroutine());
        }
    }

    private IEnumerator Explode_Coroutine()
    {
        // 1. 폭발 이펙트를 생성한다.
        Vector3 explosionPosition = transform.position;
        if (_explosionEffectPrefab != null)
        {
            ParticleSystem effect = Instantiate(_explosionEffectPrefab, explosionPosition, Quaternion.identity);
            effect.Play();
        }

        // 2. 위로 튀어오르며 빙글빙글 돈다.
        _rigidbody.AddForce(Vector3.up * _launchPower, ForceMode.Impulse);
        _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * _spinPower, ForceMode.Impulse);

        Collider[] hits = Physics.OverlapSphere(explosionPosition, _explosionRadius, _damageableMask, QueryTriggerInteraction.Ignore);
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

        // 4. 딜레이 후 사라진다.
        yield return new WaitForSeconds(_destoryDelay);
        Destroy(gameObject);
    }
}
