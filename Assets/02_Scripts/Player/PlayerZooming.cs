using UnityEngine;

public class PlayerZooming : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private int _zoomHash = Animator.StringToHash("IsZoom");

    private bool _isZooming = false;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        if (Input.GetButtonDown("Fire2"))
        {
            _isZooming = true;
            _animator.SetBool(_zoomHash, true);
        }

        if (Input.GetButtonUp("Fire2") && _isZooming)
        {
            _animator.SetBool(_zoomHash, false);
            _isZooming = false;
        }

    }
}
