using UnityEngine;


/// <summary>
/// 키보드 방향키에 따라 그 방향으로 이동하고 싶음.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Transform _moveNormal;
    [SerializeField] private float _moveSpeed = 1f;

    private void Update()
    {
        //1. 입력을 받는다

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //2. 방향을 구한다
        Vector3 direction = new Vector3(h, 0, v).normalized;
        direction = transform.TransformDirection(direction); //자기자신(transform)을 중심으로 direction을 변환
        // 로컬 좌표계 : 한 대상을 기준으로
        // 월드 좌표계 : 세상을 기준으로

        //3. 방향과 속력에 따라 이동한다.
        transform.position += direction * _moveSpeed * Time.deltaTime;
    }
}
