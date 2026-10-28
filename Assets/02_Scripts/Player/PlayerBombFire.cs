using Lean.Pool;
using UnityEngine;

/// <summary>
/// 마우스 우클릭 하면 수류탄 투척하기
/// </summary>
public class PlayerBombFire : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _grenadePrefab;
    [SerializeField] private Transform _throwPosition;

    [Header("Throw Settings")]
    [SerializeField] private int _initialBombCount = 10;
    [SerializeField] private float _maxThrowPower = 10f;
    [SerializeField] private float _minThrowPower = 2f;
    [SerializeField] private float _throwPowerIncreaseRatio = 5f;
    private float _currentThrowPower = 0f;
    private float _pendingThrowPower = 0f;
    private int _currentBombCount = 0;

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    private static readonly int GrenadeThrowHash = Animator.StringToHash("GrenadeThrow");

    private bool _isCharging = false;


    private void Awake()
    {
        _currentBombCount = _initialBombCount;
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        if (Input.GetButtonDown("Fire2") && _currentBombCount > 0)
        {
            _isCharging = true;
            _currentThrowPower = _minThrowPower;
        }

        if (_isCharging)
        {
            _currentThrowPower = Mathf.Min(_currentThrowPower + _throwPowerIncreaseRatio * Time.deltaTime, _maxThrowPower);
        }

        if (Input.GetButtonUp("Fire2") && _isCharging)
        {
            ThrowBomb();
            _isCharging = false;
        }

    }

    /// <summary>
    /// 우클릭을 놓는 순간 파워를 기억해두고 투척 애니메이션을 재생합니다.
    /// 실제 수류탄 생성은 애니메이션 이벤트(OnThrowBombEvent)에서 일어납니다.
    /// </summary>
    private void ThrowBomb()
    {
        _currentBombCount--;
        _pendingThrowPower = _currentThrowPower;

        if (_animator != null)
        {
            _animator.SetTrigger(GrenadeThrowHash);
        }
        else
        {
            // 애니메이터가 없으면 이벤트가 오지 않으므로 바로 던집니다.
            OnThrowBombEvent();
        }
    }

    /// <summary>
    /// 투척 애니메이션 클립의 ThrowBomb 이벤트에서 호출됩니다. (PlayerAnimationEvents가 중계)
    /// </summary>
    public void OnThrowBombEvent()
    {
        ThrowProjectile(_grenadePrefab, _pendingThrowPower);
    }

    private bool ThrowProjectile(GameObject projectilePrefab, float throwPower)
    {
        if (projectilePrefab == null)
        {
            GameLogger.LogWarning($"{nameof(PlayerBombFire)} : 전달받은 projectilePrefab null입니다.");
            return false;
        }

        GameObject generated = LeanPool.Spawn(projectilePrefab, _throwPosition.position, Quaternion.identity);
        if (generated == null) return false;

        if (generated.TryGetComponent(out BombProjectile proj))
        {
            proj.Throw(Camera.main.transform.forward * throwPower);
        }

        return true;
    }

}
