using UnityEngine;

public class FPS_ArmCamera : MonoBehaviour
{
    [SerializeField] private Camera _stackedCamera;

    private void Awake()
    {
        _stackedCamera = Camera.main;
    }
    private void LateUpdate()
    {
        transform.forward = _stackedCamera.transform.forward;
    }
}
