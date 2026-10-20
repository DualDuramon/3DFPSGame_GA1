using UnityEngine;

/// <summary>
/// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
/// </summary>
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private float _range = 100f;
    [SerializeField] Transform _firePosition;
    private float _damage = 10f;


    private void Update()
    {
        //1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonDown("Fire1")) //0 = 오른쪽 버튼, 1 = 왼쪽버튼, 2 = 휠클릭
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        //2. Ray를 생성하고 발사될 위치와 진행방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward); //카메라 forward 방향으로 나가는 벡터

        //3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        //4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기.
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);

        if (isHit)
        {
            GameLogger.Log($"{hitInfo.collider.name}이 총알을 맞음. 대미지 : {_damage}");
            //TODO : 파편 이펙트 생성
        }
    }
}