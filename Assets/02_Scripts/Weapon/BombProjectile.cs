using UnityEngine;

/// <summary>
/// 무언가에 부딪히면 폭발 이펙트를 남기고 사라지는 bomb
/// </summary>
public class BombProjectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private Rigidbody _rigid;

    private void Awake()
    {
        if (_rigid == null)
        {
            _rigid = GetComponent<Rigidbody>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        effect.Play();
        //TODO : 오브젝트 풀링 적용
        Destroy(gameObject);
    }

    public void Throw(Vector3 throwVector)
    {
        if (throwVector == Vector3.zero) return;

        _rigid.AddForce(throwVector, ForceMode.Impulse);
    }
}