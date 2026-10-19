using UnityEngine;

namespace CameraView
{
    internal sealed class ThirdPersonStrategy : ICameraViewStrategy
    {
        public string Name => "TPS";
        public bool UsesMouseLook => true;

        public void Enter(CameraViewSwitcher owner)
        {
            owner.SetPerspective();
        }

        public Quaternion GetViewRotation(CameraViewSwitcher owner)
        {
            return owner.MouseLookRotation;
        }

        public void UpdateCamera(CameraViewSwitcher owner)
        {
            owner.ApplyPose(GetViewRotation(owner), Vector3.back * owner.ThirdPersonDistance);
        }
    }
}
