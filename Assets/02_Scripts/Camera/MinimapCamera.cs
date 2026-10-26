using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Vector3 _offset = new Vector3(0, 8f, 0f);

    private void Awake()
    {
        if (_followTarget == null)
        {
            _followTarget = FindAnyObjectByType<PlayerMove>().transform;
        }
    }

    private void LateUpdate()
    {
        transform.position = _followTarget.position + _offset; //대상 머리 위로 옮기기
        transform.eulerAngles = new Vector3(90f, _followTarget.eulerAngles.y, 0f); //대상이 바라보는 방향대로 카메라도 같이 회전
    }
}
