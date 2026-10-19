using UnityEngine;

namespace CameraView
{
    internal interface ICameraViewStrategy
    {
        string Name { get; }
        bool UsesMouseLook { get; }
        void Enter(CameraViewSwitcher owner);
        Quaternion GetViewRotation(CameraViewSwitcher owner);
        void UpdateCamera(CameraViewSwitcher owner);
    }
}