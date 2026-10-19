using UnityEngine;

namespace CameraView
{
    internal sealed class IsometricStrategy : ICameraViewStrategy
    {
        public string Name => "Isometric View";
        public bool UsesMouseLook => false;

        public void Enter(CameraViewSwitcher owner)
        {
            owner.SetOrthographic(owner.IsometricSize);
        }

        public Quaternion GetViewRotation(CameraViewSwitcher owner)
        {
            return Quaternion.Euler(owner.IsometricAngles.x, owner.IsometricAngles.y, 0f);
        }

        public void UpdateCamera(CameraViewSwitcher owner)
        {
            owner.ApplyPose(GetViewRotation(owner), Vector3.back * owner.IsometricDistance);
        }
    }
}
