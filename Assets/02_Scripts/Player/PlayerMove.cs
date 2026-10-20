using UnityEngine;


/// <summary>
/// 카메라를 기준으로 키보드 입력 방향에 따라 지면 위에서 이동합니다.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _movementCamera;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _jumpPower = 7f;

    private const float Gravity = -9.81f;
    private float _yVelocity = 0f;


    private void Awake()
    {
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }

        if (_movementCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _movementCamera = mainCamera.transform;
            }
        }

        if (_characterController == null || _movementCamera == null)
        {
            GameLogger.LogWarning($"{nameof(PlayerMove)}: CharacterController와 이동 기준 Camera가 필요합니다.");
            enabled = false;
        }
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 카메라의 오른쪽을 기준으로 계산하면 수직 탑다운에서도 전진 방향이 유지됩니다.
        Vector3 right = Vector3.ProjectOnPlane(_movementCamera.right, Vector3.up).normalized;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        Vector3 direction = (right * h + forward * v).normalized;

        if (!_characterController.isGrounded)
        {
            _yVelocity += Gravity * Time.deltaTime;
        }
        else
        {
            if (Input.GetButtonDown("Jump")) //중재자 패턴   
            {
                _yVelocity = _jumpPower;
            }
        }

        direction.x *= _moveSpeed * Time.deltaTime;
        direction.y = _yVelocity;
        direction.z *= _moveSpeed * Time.deltaTime;

        _characterController.Move(direction);
    }
}
