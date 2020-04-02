using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool _enablePlayerInput = true;
    public bool EnablePlayerInput
    {
        get
        {
            return _enablePlayerInput;
        }
        set
        {
            // Here we can Debug.Log to debug issues with player input enabling/disabling
            _enablePlayerInput = value;
        }
    }
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
