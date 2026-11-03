using UnityEngine;

// 휴머노이드 IK로 애니메이션 위에 고개만 대상 쪽으로 돌린다
// Animator 레이어의 IK Pass가 켜져 있어야 OnAnimatorIK가 호출된다
[RequireComponent(typeof(Animator))]
public class HeadLookAt : MonoBehaviour
{
    [SerializeField] private Transform _target;

    [Header("가중치")]
    [SerializeField, Range(0f, 1f)] private float _weight = 1f;
    [SerializeField, Range(0f, 1f)] private float _bodyWeight = 0.15f;
    [SerializeField, Range(0f, 1f)] private float _headWeight = 1f;
    [SerializeField, Range(0f, 1f)] private float _eyesWeight = 0f;
    [SerializeField, Range(0f, 1f)] private float _clampWeight = 0.4f;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_target == null) return;

        _animator.SetLookAtWeight(_weight, _bodyWeight, _headWeight, _eyesWeight, _clampWeight);
        _animator.SetLookAtPosition(_target.position);
    }
}
