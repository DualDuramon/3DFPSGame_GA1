using UnityEngine;

namespace CameraView
{
    internal sealed class TopDownStrategy : ICameraViewStrategy
    {
        public string Name => "Top Down View";
        public bool UsesMouseLook => false;

        public void Enter(CameraViewSwitcher owner)
        {
            owner.SetOrthographic(owner.TopDownSize);
        }

        public Quaternion GetViewRotation(CameraViewSwitcher owner)
        {
            // +Z가 화면 위쪽이 되도록 월드 기준 회전을 고정합니다.
            return Quaternion.Euler(90f, 0f, 0f);
        }

        public void UpdateCamera(CameraViewSwitcher owner)
        {
            owner.ApplyPose(GetViewRotation(owner), Vector3.back * owner.TopDownHeight);
        }
    }
}
