using TMPro;
using UnityEngine;

/// <summary>
/// ZombieSpawner의 OnZombieCountChanged 이벤트를 구독해 현재/최대 좀비 수를 표시한다.
/// </summary>
public class UI_ZombieCountText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countText;

    private ZombieSpawner _spawner;

    private void Awake()
    {
        if (_countText == null)
        {
            _countText = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        _spawner = ZombieSpawner.Instance;
        if (_spawner == null)
        {
            GameLogger.LogWarning("ZombieSpawner를 찾을 수 없습니다.");
            return;
        }

        _spawner.OnZombieCountChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_spawner != null)
        {
            _spawner.OnZombieCountChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        _countText.text = $"좀비 {_spawner.CurrentZombieCount} / {_spawner.MaxSpawnZombieCount}";
    }
}
