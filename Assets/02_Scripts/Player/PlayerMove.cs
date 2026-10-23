using UnityEngine;


/// <summary>
/// 카메라를 기준으로 키보드 입력 방향에 따라 지면 위에서 이동합니다.
/// </summary>
[RequireComponent(typeof(PlayerCommonStatus))]
public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _movementCamera;

    private PlayerCommonStatus _status;

    private int _jumpCount = 0;
    private const float Gravity = -9.81f;
    private float _yVelocity = 0f;

    [Header("Stamina")]
    [SerializeField] private float _sprintCost = 20f;
    [SerializeField] private float _jumpCost = 20f;


    private void Awake()
    {
        _status = GetComponent<PlayerCommonStatus>();
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
        if (GameManager.Instance.GameState != GameState.Playing) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 카메라의 오른쪽을 기준으로 계산하면 수직 탑다운에서도 전진 방향이 유지됩니다.
        Vector3 right = Vector3.ProjectOnPlane(_movementCamera.right, Vector3.up).normalized;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        Vector3 direction = (right * h + forward * v).normalized;


        float needStamina = _sprintCost * Time.deltaTime;
        bool isSprint = Input.GetButton("Sprint") && _status.TryUseStamina(needStamina);
        float currentSpeed = isSprint ? _status.SprintSpeed : _status.MoveSpeed;

        direction *= currentSpeed;

        if (!_characterController.isGrounded)
        {
            _yVelocity += Gravity * Time.deltaTime;

        }
        else
        {
            _yVelocity = -1f;
            _jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump") && _jumpCount < _status.MaxJumps && _status.TryUseStamina(_jumpCost))
        {
            _yVelocity = _status.JumpPower;
            _jumpCount++;
        }

        direction.y = _yVelocity;
        _characterController.Move(direction * Time.deltaTime);
    }
}
