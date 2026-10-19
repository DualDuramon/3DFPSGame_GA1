using UnityEngine;

namespace CameraView
{
    internal sealed class ShoulderStrategy : ICameraViewStrategy
    {
        public string Name => "Shoulder View";
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
            owner.ApplyPose(GetViewRotation(owner), owner.ShoulderOffset);
        }
    }
}
