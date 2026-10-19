using UnityEngine;

namespace CameraView
{
    internal sealed class QuarterViewStrategy : ICameraViewStrategy
    {
        public string Name => "Quarter View";
        public bool UsesMouseLook => false;

        public void Enter(CameraViewSwitcher owner)
        {
            owner.SetPerspective();
        }

        public Quaternion GetViewRotation(CameraViewSwitcher owner)
        {
            return Quaternion.Euler(owner.QuarterAngles.x, owner.QuarterAngles.y, 0f);
        }

        public void UpdateCamera(CameraViewSwitcher owner)
        {
            owner.ApplyPose(GetViewRotation(owner), Vector3.back * owner.QuarterDistance);
        }
    }
}