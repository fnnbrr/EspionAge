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
    private event BlendingStartAction OnBlendingStart_Internal;
    public event BlendingCompleteAction OnBlendingComplete;
    private event BlendingCompleteAction OnBlendingComplete_Internal;  // only for internal event waiting

    private bool defaultCameraDistanceInitialized = false;
    private float activeCameraDefaultCameraDistance;  // used to keep track of the base camera distance, to then add to freely

    private bool canZoom = true;

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

    public void AddToCurrentCameraDistance(float cameraDistanceDelta)
    {
        // GetActiveCamera() == null only happens in the beginning of the game
        if (!canZoom || GetActiveCamera() == null)
        {
            return;
        }

        if (!defaultCameraDistanceInitialized)
        {
            UpdateDefaultCameraDistance(GetActiveVirtualCamera());
        }

        CinemachineFramingTransposer framingTransposer = GetActiveVirtualCamera().GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer)
        {
            framingTransposer.m_CameraDistance = activeCameraDefaultCameraDistance + cameraDistanceDelta;
        } else
        {
            Debug.LogError("Could not get the CinemachineFramingTransposer component of the current active camera!");
        }
    }

    public void BlendTo(CinemachineVirtualCamera blendToCamera, bool alertListeners = true)
    {
        BlendTo(blendToCamera, OnBlendingStart, OnBlendingComplete, alertListeners);
    }

    public void BlendTo(CinemachineVirtualCamera blendToCamera, BlendingStartAction startAction, BlendingCompleteAction completeAction, bool alertListeners = true)
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
            startAction?.Invoke(fromCamera, blendToCamera);
        }

        UpdateDefaultCameraDistance(blendToCamera);

        // Set the camera for this zone to active
        blendToCamera.gameObject.SetActive(true);

        // Whatever the current active camera is, deactivate it
        fromCamera.gameObject.SetActive(false);

        // The CinemachineBrain in the scene handles blending between the cameras 
        // - (if we set up a custom blend already)

        if (alertListeners)
        {
            StartCoroutine(WaitForBlendFinish(completeAction, fromCamera, blendToCamera));
        }
    }

    // This is just here so we can alert listeners that we have finished blending
    //  checking if the last camera is still live or not seems like the only way
    private IEnumerator WaitForBlendFinish(BlendingCompleteAction completeAction, CinemachineVirtualCamera waitForCamera, CinemachineVirtualCamera toCamera)
    {
        while(CinemachineCore.Instance.IsLive(waitForCamera))
        {
            yield return null;  // same as FixedUpdate
        }

        // alert all listeners that we stopped blending some camera
        completeAction?.Invoke(waitForCamera, toCamera);

        yield return null;
    }

    public void BlendToFarCameraForSeconds(float seconds)
    {
        StartCoroutine(BlendToFarCameraForSeconds_Internal(seconds));
    }

    private IEnumerator BlendToFarCameraForSeconds_Internal(float seconds)
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
        canZoom = false;  // disable zooming from outside sources, since the far camera should be a static distance from the player
        BlendTo(currentDistancePair.far, alertListeners: false);

        // Wait for the blending to completely finish
        while (CinemachineCore.Instance.IsLive(currentCamera))
        {
            yield return null;
        }

        // now wait the given number of seconds
        yield return new WaitForSeconds(seconds);

        // enable zooming from outside sources after we finishing zooming back in, 
        //  awareness and such can affect the player on the close camera
        OnBlendingComplete_Internal += ReenableZoomAfterBlend;

        // now blend back to the original camera
        BlendTo(currentCamera, OnBlendingStart_Internal, OnBlendingComplete_Internal);
    }

    private void ReenableZoomAfterBlend(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        OnBlendingComplete_Internal -= ReenableZoomAfterBlend;
        canZoom = true;
    }

    private void UpdateDefaultCameraDistance(CinemachineVirtualCamera virtualCamera)
    {
        if (canZoom)
        {
            defaultCameraDistanceInitialized = true;

            // Update the our local activeCameraDefaultCameraDistance
            CinemachineFramingTransposer blendToCameraFramingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (blendToCameraFramingTransposer)
            {
                activeCameraDefaultCameraDistance = blendToCameraFramingTransposer.m_CameraDistance;
            }
        }
    }
}
