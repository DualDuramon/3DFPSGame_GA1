using UnityEngine;
using UnityEngine.UI;

public class PlayerZooming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerGunFire _playerGun;
    [SerializeField] private Animator _animator;
    [SerializeField] private Image _zoomImage;

    [Header("Zoom Settings")]
    [SerializeField] private float _targetFOV = 15f;
    private float _initalFOV = 60f;

    private int _zoomHash = Animator.StringToHash("IsZoom");

    private bool _isZooming = false;

    private Camera _targetCamera;


    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if (_playerGun == null)
        {
            _playerGun = GetComponent<PlayerGunFire>();
        }
        if (_zoomImage == null)
        {
            GameLogger.LogWarning($"{name}: UI Doesn't Find!");
        }

        _targetCamera = Camera.main;
        _initalFOV = _targetCamera.fieldOfView;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing || _playerGun.WeaponMode != WeaponMode.Sniper) return;

        if (Input.GetButtonDown("Fire2"))
        {
            _isZooming = true;
            _zoomImage.gameObject.SetActive(true);
            //_animator.SetBool(_zoomHash, true);
        }


        if (Input.GetButtonUp("Fire2") && _isZooming)
        {
            _isZooming = false;
            _zoomImage.gameObject.SetActive(false);
            //_animator.SetBool(_zoomHash, false);
        }
        if (_isZooming)
        {
            _targetCamera.fieldOfView = Mathf.Lerp(_targetCamera.fieldOfView, _targetFOV, 0.15f);
        }
        else
        {
            _targetCamera.fieldOfView = Mathf.Lerp(_targetCamera.fieldOfView, _initalFOV, 0.15f);
        }

    }
}
