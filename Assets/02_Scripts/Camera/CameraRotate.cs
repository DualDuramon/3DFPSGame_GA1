using UnityEngine;

/// <summary>
/// 마우스를 움직이면 카메라를 그 방향으로 회전하고 싶다!
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 5f;

    private float _accumulationX = 0f;
    private float _accumulationY = 0f;

    private void Update()
    {
        //1. 마우스 입력 받는다
        float mouseInputX = Input.GetAxis("Mouse X");
        float mouseInputY = Input.GetAxis("Mouse Y");

        //2. 마우스 입력 만큼 회전값을 누적한다
        _accumulationX += mouseInputX * _rotationSpeed * Time.deltaTime;
        _accumulationY += mouseInputY * _rotationSpeed * Time.deltaTime;

        //3. 누적값 보정
        _accumulationY = Mathf.Clamp(_accumulationY, -90f, 90f);

        //4. 누적한 회전값을 카메라에 적용한다.
        transform.eulerAngles = new Vector3(-_accumulationY, _accumulationX, 0f);
    }
}
