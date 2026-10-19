using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _followTarget;

    private void Awake()
    {
        if (_followTarget == null)
        {
            GameLogger.LogWarning($"{typeof(CameraFollow).Name} : Target이 안보임");
        }

        transform.position = _followTarget.position;
        transform.rotation = _followTarget.rotation;
    }

    private void LateUpdate()
    {
        transform.position = _followTarget.position;
    }
}
