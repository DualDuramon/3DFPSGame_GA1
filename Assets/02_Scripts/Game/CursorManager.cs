using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;   // 마우스 커서를 화면 가운데에 고정
        Cursor.visible = false;                     // 마우스 안보이게
    }


}
