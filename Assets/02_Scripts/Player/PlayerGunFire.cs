using Lean.Pool;
using System.Collections;
using UnityEngine;

/// <summary>
/// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
/// </summary>
public class PlayerGunFire : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private float _range = 100f;

    [Header("Reloading")]
    [SerializeField] private int _maxBulletCount = 30;
    [SerializeField] private float _reloadingTime = 3f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    private bool _isReloading => _reloadCoroutine != null;
    private int _bulletCount = 0;
    private float _currentFireTime = 0f;
    private Coroutine _reloadCoroutine;

    private void Awake()
    {
        _currentFireTime = 0f;
        _bulletCount = _maxBulletCount;
    }

    private void Update()
    {
        //1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButton("Fire1")
            && Time.time - _currentFireTime > _fireInterval
            && _bulletCount > 0) //0 = 오른쪽 버튼, 1 = 왼쪽버튼, 2 = 휠클릭
        {
            Shoot();
            _bulletCount--;
            _currentFireTime = Time.time;

            if (_isReloading)
            {
                //StopAllCoroutine() //이 방식은 이 모노비헤이비어의 모든 코루틴을 종료
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }
        }

        //2. R키 누르면 총알 재장전
        if (Input.GetButtonDown("Reload") && !_isReloading)
        {
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
        }
    }

    private void Shoot()
    {
        //2. Ray를 생성하고 발사될 위치와 진행방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward); //카메라 forward 방향으로 나가는 벡터

        //3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        //4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기.
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        if (isHit)
        {
            GameLogger.Log($"{hitInfo.collider.name}이 총알을 맞음. 대미지 : {_damage}");
            if (_hitEffectPrefab != null)
            {
                LeanPool.Spawn(_hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }

            if (hitInfo.collider.TryGetComponent(out IDamageable damagable))
            {
                damagable.TakeDamage(_damage);
            }
        }
    }

    private IEnumerator Reloading_Coroutine()
    {
        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;
        _reloadCoroutine = null;
    }
}
