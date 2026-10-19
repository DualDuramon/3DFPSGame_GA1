using UnityEngine;


/// <summary>
/// 키보드 방향키에 따라 그 방향으로 이동하고 싶음.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _moveSpeed = 1f;

    private void Awake()
    {
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        //1. 입력을 받는다

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //2. 방향을 구한다
        Vector3 direction = new Vector3(h, 0, v).normalized;
        direction = transform.TransformDirection(direction); //자기자신(transform)을 중심으로 direction을 변환하고 이걸 다시 World기준으로 반환

        //3. 방향과 속력에 따라 이동한다.
        //transform.position += direction * _moveSpeed * Time.deltaTime;
        _characterController.Move(direction * _moveSpeed * Time.deltaTime);
    }
}
