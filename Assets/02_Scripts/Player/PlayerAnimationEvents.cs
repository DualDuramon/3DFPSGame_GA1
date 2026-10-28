using UnityEngine;

/// <summary>
/// Animator가 붙은 모델 오브젝트에서 애니메이션 이벤트를 받아 플레이어 컴포넌트로 전달합니다.
/// (애니메이션 이벤트는 Animator와 같은 오브젝트의 컴포넌트만 호출할 수 있습니다.)
/// </summary>
public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private PlayerBombFire _bombFire;

    private void Awake()
    {
        if (_bombFire == null)
        {
            _bombFire = GetComponentInParent<PlayerBombFire>();
        }
    }

    // 투척 클립의 "ThrowBomb" 이벤트
    public void ThrowBomb()
    {
        if (_bombFire != null)
        {
            _bombFire.OnThrowBombEvent();
        }
    }
}
