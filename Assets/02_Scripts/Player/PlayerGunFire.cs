using Lean.Pool;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
/// </summary>
[RequireComponent(typeof(PlayerCommonStatus))]
public class PlayerGunFire : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCommonStatus _status;

    [SerializeField] private WeaponMode _weaponMode;

    [Header("Reloading")]
    [SerializeField] private int _maxBulletCount = 30;
    [SerializeField] private float _reloadingTime = 3f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [Header("Bullet Trail")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private BulletTrail _bulletTrail;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    [Header("MuzzleFlahs")]
    [SerializeField] private GameObject[] _muzzleFlashes;
    [SerializeField] private float _flashTime = 0.05f;

    public event Action OnGunFired; //current Bullet / Max Bullet
    public event Action OnWeaponModeSwitched; //currentSwitchedMode
    public event Action OnWeaponReloadStart;
    public event Action OnWeaponReloadEnd;

    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    private bool _isReloading => _reloadCoroutine != null;
    private int _bulletCount = 0;
    private float _currentFireTime = 0f;
    private Coroutine _reloadCoroutine;
    public WeaponMode WeaponMode => _weaponMode;
    public int MaxBulletCount => _maxBulletCount;
    public int CurrentBulletCount => _bulletCount;


    private void Awake()
    {
        _status = GetComponent<PlayerCommonStatus>();
        _currentFireTime = 0f;
        _bulletCount = _maxBulletCount;
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        bool isMouseVisible = Cursor.visible;
        bool canAttack = Input.GetButton("Fire1") && !isMouseVisible && _bulletCount > 0;

        // 발사 버튼을 누르고 있는 동안 상체 사격 애니메이션을 유지합니다.
        if (_animator != null)
        {
            _animator.SetBool(IsAttackingHash, canAttack);
        }

        //1. 마우스 오른쪽 버튼 입력 받기
        if (canAttack
            && Time.time - _currentFireTime > _status.FireInterval) //0 = 오른쪽 버튼, 1 = 왼쪽버튼, 2 = 휠클릭
        {
            Shoot();
            _bulletCount--;
            _currentFireTime = Time.time;

            OnGunFired?.Invoke();

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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeWeaponMode(WeaponMode.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeWeaponMode(WeaponMode.Sniper);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeWeaponMode(WeaponMode.Melee);
        }
    }

    private void ChangeWeaponMode(WeaponMode newMode)
    {
        _weaponMode = newMode;
        OnWeaponModeSwitched?.Invoke();
    }

    private void Shoot()
    {

        //2. Ray를 생성하고 발사될 위치와 진행방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward); //카메라 forward 방향으로 나가는 벡터

        //3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        //4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기.
        bool isHit = Physics.Raycast(ray, out hitInfo, _status.Range);

        //총구에서 적중 지점(빗나가면 사거리 끝)까지 궤적 재생
        if (_bulletTrail != null && _muzzle != null)
        {
            Vector3 endPoint = isHit ? hitInfo.point : ray.origin + ray.direction * _status.Range;
            _bulletTrail.Play(_muzzle.position, endPoint);
        }

        if (isHit)
        {
            GameLogger.Log($"{hitInfo.collider.name}이 총알을 맞음. 대미지 : {_status.Damage}");
            if (_hitEffectPrefab != null)
            {
                LeanPool.Spawn(_hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }

            if (hitInfo.collider.TryGetComponent(out IDamageable damagable))
            {
                DamageInfo info = new DamageInfo(_status.Damage, hitInfo.point, hitInfo.normal);

                damagable.TakeDamage(info);
            }
        }

        StartCoroutine(MuzzleFlash_Coroutine());
    }

    private IEnumerator Reloading_Coroutine()
    {
        _bulletCount = 0;
        OnWeaponReloadStart?.Invoke();
        yield return new WaitForSeconds(_reloadingTime);

        _bulletCount = _maxBulletCount;
        OnWeaponReloadEnd?.Invoke();
        _reloadCoroutine = null;
    }

    private IEnumerator MuzzleFlash_Coroutine() //코루틴으로 flash 하는것도 정답은 아니다!
    {
        int randomNum = UnityEngine.Random.Range(0, _muzzleFlashes.Length);

        _muzzleFlashes[randomNum].SetActive(true);

        yield return new WaitForSeconds(_flashTime);

        _muzzleFlashes[randomNum].SetActive(false);
    }
}
