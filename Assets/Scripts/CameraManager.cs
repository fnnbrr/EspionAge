using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineBrain brain;
    private void Start()
    {
        if (!brain && !(brain = FindObjectOfType<CinemachineBrain>()))
        {
            Utils.LogErrorAndStopPlayMode($"{name} needs a CinemachineBrain component! Could not fix automatically.");
        }
    }

    public ICinemachineCamera GetActiveCamera()
    {
        return brain.ActiveVirtualCamera;
    }

    public Transform GetActiveCameraTransform()
    {
        return GetActiveCamera().VirtualCameraGameObject.transform;
    }
}
