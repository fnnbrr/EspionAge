using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    public float closedDelta = 5f;

    private float closedYRotation;
    private bool isClosed = true;

    public delegate void OpenDoorAction();
    public delegate void CloseDoorAction();
    public event OpenDoorAction OnDoorOpen;
    public event CloseDoorAction OnDoorClose;

    private void Start()
    {
        closedYRotation = transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        if (Mathf.Abs(closedYRotation - transform.rotation.eulerAngles.y) <= closedDelta)
        {
            if (!isClosed)
            {
                OnDoorClose?.Invoke();
                isClosed = true;
            }
        } 
        else
        {
            if (isClosed)
            {
                OnDoorOpen?.Invoke();
                isClosed = false;
            }
        }
    }
}
