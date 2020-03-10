using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform CMCamerasRoot;

    // No events for OnBlendingStart / OnBlendingComplete so we have to make our own
    public delegate void BlendingStartAction(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera);
    public delegate void BlendingCompleteAction(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera);
    public event BlendingStartAction OnBlendingStart;
    private event BlendingStartAction OnBlendingStart_Internal;
    public event BlendingCompleteAction OnBlendingComplete;
    private event BlendingCompleteAction OnBlendingComplete_Internal;  // only for internal event waiting

    private bool canZoom = true;

    private Dictionary<CinemachineVirtualCamera, float> defaultCameraDistanceMapping;  // keeps track of what distance all cameras started with

    private void Start()
    {
        if (!brain && !(brain = FindObjectOfType<CinemachineBrain>()))
        {
            Utils.LogErrorAndStopPlayMode($"{name} needs a CinemachineBrain component! Could not fix automatically.");
        }

        OnBlendingStart_Internal += DisableZoomOnBlendStart_Internal;
        OnBlendingComplete_Internal += EnableZoomOnBlendEnd_Internal;

        InitializeDefaultCameraDistanceMapping();
    }

    private void InitializeDefaultCameraDistanceMapping()
    {
        defaultCameraDistanceMapping = new Dictionary<CinemachineVirtualCamera, float>();
        foreach (CinemachineVirtualCamera virtualCamera in FindObjectsOfType<CinemachineVirtualCamera>())
        {
            UpdateDefaultCameraDistance(virtualCamera);
        }
    }

    private void DisableZoomOnBlendStart_Internal(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        canZoom = false;
    }

    private void EnableZoomOnBlendEnd_Internal(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        canZoom = true;
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
        if (!canZoom || GetActiveCamera() == null) return;

        CinemachineVirtualCamera currentVirtualCamera = GetActiveVirtualCamera();
        if (defaultCameraDistanceMapping.TryGetValue(currentVirtualCamera, out float defaultCameraDistance))
        {
            CinemachineFramingTransposer framingTransposer = GetCameraFramingTransposer(currentVirtualCamera);
            if (framingTransposer)
            {
                framingTransposer.m_CameraDistance = defaultCameraDistance + cameraDistanceDelta;
            }
        }
    }

    public void BlendTo(CinemachineVirtualCamera blendToCamera, bool alertGlobally = true, bool doHardBlend = false)
    {
        // This can fail when we first start the game, so lets check for it
        if (!IsActiveCameraValid()) return;

        CinemachineVirtualCamera fromCamera = GetActiveVirtualCamera();
        // early exit, because we cannot blend between the same cameras??
        if (!fromCamera || blendToCamera == fromCamera) return;

        UpdateDefaultCameraDistance(blendToCamera);

        canZoom = false;

        // alert all listeners that we started blending some camera
        OnBlendingStart_Internal?.Invoke(fromCamera, blendToCamera);
        if (alertGlobally)
        {
            OnBlendingStart?.Invoke(fromCamera, blendToCamera);
        }

        CinemachineBlenderSettings.CustomBlend[] currentBlendSettings = brain.m_CustomBlends.m_CustomBlends;
        if (doHardBlend)
        {
            CinemachineBlenderSettings.CustomBlend hardBlend = new CinemachineBlenderSettings.CustomBlend
            {
                m_From = CinemachineBlenderSettings.kBlendFromAnyCameraLabel,
                m_To = blendToCamera.name,
                m_Blend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f)
            };
            brain.m_CustomBlends.m_CustomBlends = brain.m_CustomBlends.m_CustomBlends.Append(hardBlend).ToArray();
        }

        // Set the camera for this zone to active
        blendToCamera.gameObject.SetActive(true);

        // Whatever the current active camera is, deactivate it
        fromCamera.gameObject.SetActive(false);

        // The CinemachineBrain in the scene handles blending between the cameras 
        // - (if we set up a custom blend already)

        StartCoroutine(WaitForBlendFinish(fromCamera, blendToCamera, alertGlobally, currentBlendSettings));
    }

    // This is just here so we can alert listeners that we have finished blending
    //  checking if the last camera is still live or not seems like the only way
    private IEnumerator WaitForBlendFinish(CinemachineVirtualCamera waitForCamera, CinemachineVirtualCamera toCamera, bool alertGlobalListeners, CinemachineBlenderSettings.CustomBlend[] blendSettings)
    {
        while(CinemachineCore.Instance.IsLive(waitForCamera))
        {
            yield return null;  // same as FixedUpdate
        }

        brain.m_CustomBlends.m_CustomBlends = blendSettings;

        // alert all global listeners that we stopped blending some camera
        if (alertGlobalListeners)
        {
            OnBlendingComplete?.Invoke(waitForCamera, toCamera);
        }
        OnBlendingComplete_Internal?.Invoke(waitForCamera, toCamera);

        yield return null;
    }

    public Coroutine BlendToCameraPrefabForSeconds(GameObject cameraPrefab, float seconds)
    {
        return StartCoroutine(BlendToCameraPrefabForSeconds_Internal(cameraPrefab, seconds));
    }

    private IEnumerator BlendToCameraPrefabForSeconds_Internal(GameObject cameraPrefab, float seconds)
    {
        // First see if we have a registered distance pair for the current camera
        CinemachineVirtualCamera currentCamera = GetActiveVirtualCamera();

        GameObject cameraInstance = SpawnCameraFromPrefab(cameraPrefab);
        CinemachineVirtualCamera virtualCamera = cameraInstance.GetComponentInChildren<CinemachineVirtualCamera>();
        if (!virtualCamera) yield break;

        // Start blending to the far camera (and do not alert global listeners of this)
        BlendTo(virtualCamera, alertGlobally: false);

        // Wait for the blending to completely finish
        while (CinemachineCore.Instance.IsLive(currentCamera))
        {
            yield return null;
        }

        // now wait the given number of seconds
        yield return new WaitForSeconds(seconds);

        // now blend back to the original camera
        BlendTo(currentCamera, alertGlobally: false);

        // Wait for the blending to completely finish
        while (CinemachineCore.Instance.IsLive(currentCamera))
        {
            yield return null;
        }
    }

    public GameObject SpawnCameraFromPrefab(GameObject prefab)
    {
        return Instantiate(prefab, CMCamerasRoot);
    }

    private CinemachineFramingTransposer GetCameraFramingTransposer(CinemachineVirtualCamera virtualCamera)
    {
        // Update the our local activeCameraDefaultCameraDistance
        return virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    private float GetCameraDistance(CinemachineFramingTransposer framingTransposer)
    {
        return framingTransposer.m_CameraDistance;
    }

    private void UpdateDefaultCameraDistance(CinemachineVirtualCamera virtualCamera)
    {
        if (!defaultCameraDistanceMapping.ContainsKey(virtualCamera))
        {
            CinemachineFramingTransposer framingTransposer = GetCameraFramingTransposer(virtualCamera);
            if (framingTransposer)
            {
                defaultCameraDistanceMapping.Add(virtualCamera, GetCameraDistance(framingTransposer));
            }
        }
    }
}
