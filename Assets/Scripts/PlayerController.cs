using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool EnablePlayerInput { get; set; } = true;
    public bool controllerConnected = false;

    private void Start()
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

    public Vector3 AlignDirectionWithCamera(Vector3 initialDirection)
    {
        return Quaternion.Euler(0, CameraManager.Instance.brain.transform.eulerAngles.y, 0) * initialDirection;
    }
}
