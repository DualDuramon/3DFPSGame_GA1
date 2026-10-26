using Lean.Pool;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 일정 간격으로 좀비를 Lean Pool에서 꺼내 스폰한다.
/// </summary>
public class ZombieSpawner : MonoBehaviour
{
    private static ZombieSpawner _instance;

    public static ZombieSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<ZombieSpawner>();
            }

            return _instance;
        }
    }

    [Header("Reference")]
    [SerializeField] private GameObject _zombiePrefab;
    [Tooltip("비어 있으면 스포너 위치를 중심으로 스폰한다")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Spawn Status")]
    [SerializeField] private int _totalZombieSpawnCount = 0;
    [SerializeField] private int _maxZombieSpawnCount = 10;

    [Header("SpawnSetting")]
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private float _spawnRadius = 3f;

    private int _currentZombieCount = 0;
    public int CurrentZombieCount => _currentZombieCount;
    public int MaxSpawnZombieCount => _maxZombieSpawnCount;

    public event Action OnZombieCountChanged = null;

    private float _spawnTimer;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;
        if (_zombiePrefab == null) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnInterval) return;

        if (_totalZombieSpawnCount >= _maxZombieSpawnCount) return;

        _spawnTimer = 0f;
        SpawnZombie();
    }

    private void SpawnZombie()
    {
        Vector3 center = transform.position;
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            if (point != null) center = point.position;
        }

        Vector2 offset = Random.insideUnitCircle * _spawnRadius;
        Vector3 position = center + new Vector3(offset.x, 0f, offset.y);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        LeanPool.Spawn(_zombiePrefab, position, rotation);
        IncreaseZomebieCount();
        GameLogger.Log($"좀비 스폰: {position}");
    }

    public void DeSpawnZombie(GameObject clone)
    {
        LeanPool.Despawn(clone);
    }

    public void IncreaseZomebieCount()
    {
        _currentZombieCount += 1;
        _totalZombieSpawnCount += 1;
        OnZombieCountChanged?.Invoke();
    }

    public void DecreaseZombieCount()
    {
        _currentZombieCount -= 1;
        OnZombieCountChanged?.Invoke();
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            foreach (Transform point in _spawnPoints)
            {
                if (point != null) Gizmos.DrawWireSphere(point.position, _spawnRadius);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
        }
    }
}
