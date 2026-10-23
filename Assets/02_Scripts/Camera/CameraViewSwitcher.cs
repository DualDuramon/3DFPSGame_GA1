using UnityEngine;

namespace CameraView
{
    [RequireComponent(typeof(Camera))]
    public class CameraViewSwitcher : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _followTarget;

        [Header("Mouse Look")]
        [SerializeField] private float _rotationSpeed = 200f;
        [SerializeField] private float _pitchLimit = 80f;

        [Header("Perspective")]
        [SerializeField] private float _fieldOfView = 60f;
        [SerializeField] private float _thirdPersonDistance = 4f;
        [SerializeField] private Vector3 _shoulderOffset = new Vector3(0.7f, -0.25f, -2.5f);

        [Header("Quarter View")]
        [SerializeField] private Vector2 _quarterAngles = new Vector2(45f, 45f);
        [SerializeField] private float _quarterDistance = 10f;

        [Header("Isometric View")]
        [SerializeField] private Vector2 _isometricAngles = new Vector2(35.264f, 45f);
        [SerializeField] private float _isometricDistance = 10f;
        [SerializeField] private float _isometricSize = 5f;

        [Header("Top Down View")]
        [SerializeField] private float _topDownHeight = 12f;
        [SerializeField] private float _topDownSize = 7f;

        private Camera _camera;
        private Renderer[] _playerRenderers;
        private bool[] _originalRendererStates;
        private ICameraViewStrategy[] _strategies;
        private int _currentViewIndex;
        private float _pitch; //현재 카메라의 위 아래로 기울어지는 각도

        private ICameraViewStrategy CurrentStrategy => _strategies[_currentViewIndex];
        internal Quaternion MouseLookRotation => Quaternion.Euler(-_pitch, _player.eulerAngles.y, 0f);

        internal float ThirdPersonDistance => _thirdPersonDistance;
        internal Vector3 ShoulderOffset => _shoulderOffset;
        internal Vector2 QuarterAngles => _quarterAngles;
        internal float QuarterDistance => _quarterDistance;
        internal Vector2 IsometricAngles => _isometricAngles;
        internal float IsometricDistance => _isometricDistance;
        internal float IsometricSize => _isometricSize;
        internal float TopDownHeight => _topDownHeight;
        internal float TopDownSize => _topDownSize;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            if (_camera == null || _player == null || _followTarget == null)
            {
                GameLogger.LogWarning($"{nameof(CameraViewSwitcher)}: Camera, Player, Follow Target이 필요합니다.");
                enabled = false;
                return;
            }

            _playerRenderers = _player.GetComponentsInChildren<Renderer>(true);
            _originalRendererStates = new bool[_playerRenderers.Length];
            for (int i = 0; i < _playerRenderers.Length; i++)
            {
                _originalRendererStates[i] = _playerRenderers[i].enabled;
            }

            // V키 순서대로 전략을 한 번만 생성합니다.
            _strategies = new ICameraViewStrategy[]
            {
                new FirstPersonStrategy(),
                new ThirdPersonStrategy(),
                new ShoulderStrategy(),
                new QuarterViewStrategy(),
                new IsometricStrategy(),
                new TopDownStrategy()
            };
        }

        private void OnEnable()
        {
            if (_strategies != null)
            {
                SwitchView(_currentViewIndex);
            }
        }

        private void OnDisable()
        {
            if (_playerRenderers != null)
            {
                SetPlayerVisible(true);
            }
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Playing) return;

            if (Input.GetKeyDown(KeyCode.V))
            {
                SwitchView((_currentViewIndex + 1) % _strategies.Length);
            }

            // 고정 시점에서는 상하 입력을 누적하지 않고 이전 각도를 보존합니다.
            if (CurrentStrategy.UsesMouseLook)
            {
                _pitch += Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
                _pitch = Mathf.Clamp(_pitch, -_pitchLimit, _pitchLimit);
            }

            // 이동 전에 이번 프레임의 시점 회전을 반영하고, 위치는 LateUpdate에서 추적합니다.
            transform.rotation = CurrentStrategy.GetViewRotation(this);
        }

        private void LateUpdate()
        {
            CurrentStrategy.UpdateCamera(this);
        }

        private void SwitchView(int index)
        {
            _currentViewIndex = index;
            SetPlayerVisible(index != 0);
            CurrentStrategy.Enter(this);
            CurrentStrategy.UpdateCamera(this);
            GameLogger.Log($"{nameof(CameraViewSwitcher)}: {CurrentStrategy.Name}");
        }

        /// <summary>
        /// 플레이어의 Renderer를 종료하여 특정 카메라 뷰에서 플레이어의 mesh를 안보이게 함
        /// </summary>
        /// <param name="visible"></param>
        private void SetPlayerVisible(bool visible)
        {
            for (int i = 0; i < _playerRenderers.Length; i++)
            {
                if (_playerRenderers[i] == null) continue;
                _playerRenderers[i].enabled = visible && _originalRendererStates[i];
            }
        }

        /// <summary>
        /// 카메라의 원근감 있게(Perspective옵션) 활성화
        /// </summary>
        internal void SetPerspective()
        {
            _camera.orthographic = false;
            _camera.fieldOfView = _fieldOfView;
        }

        /// <summary>
        /// 카메라를 원근감 없게(Orthographic) 전환
        /// </summary>
        /// <param name="size"></param>
        internal void SetOrthographic(float size)
        {
            _camera.orthographic = true;
            _camera.orthographicSize = size;
        }

        /// <summary>
        /// 추적하는 Target을 기준으로 위치 오프셋 및 회전 적용
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="offset"></param>
        internal void ApplyPose(Quaternion rotation, Vector3 offset)
        {
            transform.SetPositionAndRotation(_followTarget.position + rotation * offset, rotation);
        }
    }
}
