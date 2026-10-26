using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private MinimapCamera _camera;


    private void Awake()
    {
        _camera = FindAnyObjectByType<MinimapCamera>();
    }

    public void HandleZoomInButtonClicked()
    {
        _camera.CameraZoomIn();

    }

    public void HandleZoomOutButtonClicked()
    {
        _camera.CameraZoomOut();
    }
}
