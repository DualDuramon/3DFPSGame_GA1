using UnityEngine;


/// <summary>
/// 카메라를 기준으로 키보드 입력 방향에 따라 지면 위에서 이동합니다.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _movementCamera;

    [Header("Move Control")]
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _runSpeed = 12f;


    [Header("Jump Control")]
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private int _maxJumpCount = 2;
    private int _currentJumpCount = 0;
    private const float Gravity = -9.81f;
    private float _yVelocity = 0f;

    [Header("Stamina Control")]
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _staminaRestoreRate = 100f;
    [SerializeField] private float _runStaminaDecresementRate = 10f;
    [SerializeField] private float _jumpStaminaDecresement = 30f;
    [SerializeField] private float _currentStamina = 100f;
    [SerializeField] private bool _needSomeRest = false;
    private const float Fully_Rested_Normal = 5f;


    public float CurrentStaminaRatio => _currentStamina / _maxStamina;
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

        Vector3 right = Vector3.ProjectOnPlane(_movementCamera.right, Vector3.up).normalized;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        Vector3 direction = (right * h + forward * v).normalized;

        if (Input.GetButton("LeftShift") && !_needSomeRest && direction != Vector3.zero)
        {
            direction *= _runSpeed;
            _currentStamina -= _runStaminaDecresementRate * Time.deltaTime;

            if (_currentStamina < 0f)
            {
                _needSomeRest = true;
            }
        }
        else
        {
            direction *= _moveSpeed;
            HandleRestoreStaminaPerRate();
        }


        if (!_characterController.isGrounded)
        {
            _yVelocity += Gravity * Time.deltaTime;
        }
        else
        {
            _yVelocity = 0f;
            _currentJumpCount = 0;
        }

        if (Input.GetButtonDown("Jump") && !_needSomeRest && _currentStamina > _jumpStaminaDecresement) //중재자 패턴   
        {
            if (_characterController.isGrounded || _currentJumpCount < _maxJumpCount)
            {
                _yVelocity = _jumpPower;
                _currentJumpCount++;
                _currentStamina -= _jumpStaminaDecresement;
            }
        }
        direction.y = _yVelocity;
        _characterController.Move(direction * Time.deltaTime);

    }

    private void HandleRestoreStaminaPerRate()
    {
        _currentStamina = Mathf.Clamp(_currentStamina + _staminaRestoreRate * Time.deltaTime, 0f, 100f);

        if (_needSomeRest && _currentStamina > Fully_Rested_Normal)
        {
            _needSomeRest = false;
        }
    }
}
