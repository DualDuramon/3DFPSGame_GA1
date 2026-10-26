using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    {
        LockCursor();
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            LockCursor();
        }
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;   // 마우스 커서를 화면 가운데에 고정
        Cursor.visible = false;                     // 마우스 안보이게
    }
    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;   // 마우스 커서를 화면 가운데에 고정
        Cursor.visible = true;                     // 마우스 안보이게
    }

}
