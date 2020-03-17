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

    [Header("FMOD Audio")]
    [FMODUnity.EventRef]
    public string DoorSqueak;
    [FMODUnity.EventRef]
    public string DoorSlam;
    public FMODUnity.StudioEventEmitter TutorialMusic;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        TutorialMusic = GetComponent<FMODUnity.StudioEventEmitter>();
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
                    FMODUnity.RuntimeManager.PlayOneShot(DoorSlam, transform.position);
                    TutorialMusic.Play();
                }
            }
        }
        else
        {
            if (isClosed)
            {
                OnDoorOpen?.Invoke();
                FMODUnity.RuntimeManager.PlayOneShot(DoorSqueak, transform.position);
                isClosed = false;
            }
        }
    }
}
