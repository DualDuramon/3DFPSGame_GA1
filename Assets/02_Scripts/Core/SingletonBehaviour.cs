using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : class
{
    private static T _instance = null;
    public static T Instance => _instance;

    // false면 씬 오브젝트로 동작해 씬이 다시 로드될 때 함께 파괴·재생성된다 (씬 내 UI 참조를 들고 있는 경우)
    protected virtual bool PersistAcrossScenes => true;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (PersistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (ReferenceEquals(_instance, this))
        {
            _instance = null;
        }
    }
}
