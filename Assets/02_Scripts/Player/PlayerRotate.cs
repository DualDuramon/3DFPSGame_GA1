using UnityEngine;

[RequireComponent(typeof(PlayerCommonStatus))]
public class PlayerRotate : MonoBehaviour
{
    private PlayerCommonStatus _status;
    private float _accumulationX = 0f;

    private void Awake()
    {
        _status = GetComponent<PlayerCommonStatus>();
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        float mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * _status.RotationSpeed * Time.deltaTime;

        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }

}
