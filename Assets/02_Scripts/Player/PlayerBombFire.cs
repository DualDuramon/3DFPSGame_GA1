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
    private int _currentBombCount = 0;

    private bool _isBombFused = false;

    private void Awake()
    {
        _currentBombCount = _initialBombCount;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire2") && _currentBombCount > 0)
        {
            _isBombFused = true;
            _currentThrowPower = _minThrowPower;
        }

        if (_isBombFused)
        {
            _currentThrowPower = Mathf.Min(_currentThrowPower + _throwPowerIncreaseRatio * Time.deltaTime, _maxThrowPower);
        }

        if (Input.GetButtonUp("Fire2") && _isBombFused)
        {
            ThrowBomb();
            _isBombFused = false;
        }

    }

    private void ThrowBomb()
    {
        ThrowProjectile(_grenadePrefab);
        _currentBombCount--;
    }
    private void ThrowProjectile(GameObject projectilePrefab)
    {
        if (projectilePrefab == null)
        {
            GameLogger.LogWarning($"{nameof(PlayerBombFire)} : 전달받은 projectilePrefab null입니다.");
            return;
        }

        GameObject generated = Instantiate(projectilePrefab, _throwPosition.position, Quaternion.identity);

        if (generated.TryGetComponent(out BombProjectile proj))
        {
            proj.Throw(Camera.main.transform.forward * _currentThrowPower);
        }

    }

}
