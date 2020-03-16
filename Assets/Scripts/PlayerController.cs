using System.Linq;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public bool EnablePlayerInput { get; set; } = true;
    public bool controllerConnected = false;

    void Start()
    {
        SetControllerConnected();
    }

    private void SetControllerConnected()
    {
        string[] controllerNames = Input.GetJoystickNames();

        if (controllerNames.Length <= 0 || controllerNames.All(string.IsNullOrEmpty))
        {
            controllerConnected = false;
        }
        else
        {
            controllerConnected = true;
            print("CONNECTED: " + controllerNames[0]);
        }
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
