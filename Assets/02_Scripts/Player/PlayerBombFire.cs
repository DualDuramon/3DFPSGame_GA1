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
    private int _currentBombCount = 0;
    [SerializeField] private float _throwPower;

    private void Awake()
    {
        _currentBombCount = _initialBombCount;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire2") && _currentBombCount > 0)
        {
            ThrowProjectile(_grenadePrefab);
            _currentBombCount--;
        }
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
            proj.Throw(Camera.main.transform.forward * _throwPower);
        }

    }

}
