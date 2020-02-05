using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[System.Serializable]
public class VirtualCameraPair
{
    public CinemachineVirtualCamera normal;
    public CinemachineVirtualCamera far;
}

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineBrain brain;

    // Specific pairs of cameras which we can dynamically blend between during certain events
    public List<VirtualCameraPair> distancePairs;

    // No events for OnBlendingStart / OnBlendingComplete so we have to make our own
    public delegate void BlendingStartAction(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera);
    public delegate void BlendingCompleteAction(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera);
    public event BlendingStartAction OnBlendingStart;
    public event BlendingCompleteAction OnBlendingComplete;

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

    public CinemachineVirtualCamera GetActiveVirtualCamera()
    {
        return GetActiveCamera().VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }

    public bool IsActiveCameraValid()
    {
        return GetActiveCamera() != null && GetActiveCamera().IsValid;
    }

    public Transform GetActiveCameraTransform()
    {
        return GetActiveCamera().VirtualCameraGameObject.transform;
    }

    public void BlendTo(CinemachineVirtualCamera blendToCamera, bool alertListeners = true)
    {
        // This can fail when we first start the game, so lets check for it
        if (!IsActiveCameraValid())
        {
            return;
        }

        CinemachineVirtualCamera fromCamera = GetActiveVirtualCamera();
        if (!fromCamera || blendToCamera == fromCamera)
        {
            return;  // early exit, because we cannot blend between the same cameras??
        }

        // alert all listeners that we started blending some camera
        if (alertListeners)
        {
            OnBlendingStart?.Invoke(fromCamera, blendToCamera);
        }

        // Set the camera for this zone to active
        blendToCamera.gameObject.SetActive(true);

        // Whatever the current active camera is, deactivate it
        fromCamera.gameObject.SetActive(false);

        // The CinemachineBrain in the scene handles blending between the cameras 
        // - (if we set up a custom blend already)

        if (alertListeners)
        {
            StartCoroutine(WaitForBlendFinish(fromCamera, blendToCamera));
        }
    }

    // This is just here so we can alert listeners that we have finished blending
    //  checking if the last camera is still live or not seems like the only way
    private IEnumerator WaitForBlendFinish(CinemachineVirtualCamera waitForCamera, CinemachineVirtualCamera toCamera)
    {
        while(CinemachineCore.Instance.IsLive(waitForCamera))
        {
            yield return null;  // same as FixedUpdate
        }

        // alert all listeners that we stopped blending some camera
        OnBlendingComplete?.Invoke(waitForCamera, toCamera);

        yield return null;
    }

    public IEnumerator BlendToFarCameraForSeconds(float seconds)
    {
        // First see if we have a registered distance pair for the current camera
        CinemachineVirtualCamera currentCamera = GetActiveVirtualCamera();
        VirtualCameraPair currentDistancePair = distancePairs.Find(pair => pair.normal == currentCamera);

        if (currentDistancePair == null)
        {
            yield break;
        }

        // Start blending to the far camera (and do not alert listeners of this)
        // TODO: Think of a better way to handle this
        BlendTo(currentDistancePair.far, false);

        // Wait for the blending to completely finish
        while (CinemachineCore.Instance.IsLive(currentCamera))
        {
            yield return null;
        }

        // now wait the given number of seconds
        yield return new WaitForSeconds(seconds);

        // now blend back to the original camera
        BlendTo(currentCamera, false);
    }
}
