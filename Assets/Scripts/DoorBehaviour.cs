using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DoorBehaviour : MonoBehaviour
{
    public float closedDelta = 5f;
    public bool shakeCameraOnClose = true;

    private float closedYRotation;
    private bool isClosed = true;

    private CinemachineImpulseSource impulseSource;

    public delegate void OpenDoorAction();
    public delegate void CloseDoorAction();
    public event OpenDoorAction OnDoorOpen;
    public event CloseDoorAction OnDoorClose;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

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
                if (shakeCameraOnClose && impulseSource)
                {
                    impulseSource.GenerateImpulse();
                }
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
