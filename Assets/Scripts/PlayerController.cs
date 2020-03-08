using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public bool EnablePlayerInput { get; set; } = true;
    public bool controllerConnected = false;

    void Start()
    {
        if (CameraManager.Instance)
        {
            CameraManager.Instance.OnBlendingStart += HandleCameraOnBlendingStart;
            CameraManager.Instance.OnBlendingComplete += HandleCameraOnBlendingComplete;
        }

        controllerConnected = Input.GetJoystickNames().Length > 0;
    }

    private void HandleCameraOnBlendingStart(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        EnablePlayerInput = false;
    }

    private void HandleCameraOnBlendingComplete(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        EnablePlayerInput = true;
    }
}
