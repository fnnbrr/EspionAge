using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RegionManager : Singleton<RegionManager>
{
    [Header("Camera Zones")]
    public CameraZone diningArea;
    public CameraZone kitchen;
    public CameraZone hallway;
    public CameraZone birdiesRoom;

    [Header("Region Triggers")]
    public RegionTrigger finalHallwayDoor;

    // Treating this list as a stack where the last element is considered "current" zone
    private List<CameraZone> currentPlayerZones;

    public delegate void EnterZoneAction(CameraZone zone);
    public delegate void ExitZoneAction(CameraZone zone);
    public event EnterZoneAction OnPlayerEnterZone;
    public event ExitZoneAction OnPlayerExitZone;

    private void Awake()
    {
        currentPlayerZones = new List<CameraZone>();
    }

    public bool PlayerIsInAZone()
    {
        return currentPlayerZones.Count > 0;
    }

    public CameraZone GetCurrentZone()
    {
        if (!PlayerIsInAZone())
        {
            return null;
        }
        return currentPlayerZones[currentPlayerZones.Count - 1];
    }

    public void ReportPlayerEnterZone(CameraZone zone)
    {
        currentPlayerZones.Add(zone);
        // OnPlayerEnterZone will be called after camera blending is done within HandleCurentZone

        HandleCurrentZone();
    }

    public void ReportPlayerExitZone(CameraZone zone)
    {
        currentPlayerZones.Remove(zone);
        OnPlayerExitZone?.Invoke(zone);

        HandleCurrentZone();
    }

    private void HandleCurrentZone()
    {
        CameraZone currentZone = GetCurrentZone();
        if (!currentZone) return;

        HandleRestrictedZone(currentZone.isRestricted);
        HandleCameraChange(currentZone.mainCamera);
    }

    private void HandleRestrictedZone(bool isRestricted)
    {
        if (isRestricted)
        {
            UIManager.Instance.staminaBar.FadeIn();
        }
        else
        {
            UIManager.Instance.staminaBar.FadeOut();
        }
    }

    private void HandleCameraChange(CinemachineVirtualCamera camera)
    {
        if (camera)
        {
            CameraManager.Instance.OnBlendingComplete += AlertBlendingComplete;
            CameraManager.Instance.BlendTo(camera);
        }
    }

    private void AlertBlendingComplete(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        if (PlayerIsInAZone() && toCamera == GetCurrentZone().mainCamera)  // we do not want to care about unrelated camera blending events
        {
            CameraManager.Instance.OnBlendingComplete -= AlertBlendingComplete;
            OnPlayerEnterZone?.Invoke(GetCurrentZone());
        }
    }
}
