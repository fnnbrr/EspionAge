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
    public CameraZone nursesRoom;

    [Header("Region Triggers")]
    public RegionTrigger finalHallwayDoor;

    [Header("Doors")]
    public DoorBehaviour nurseRoomDoor;

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

    public bool PlayerIsInZone(CameraZone zone)
    {
        return currentPlayerZones.Contains(zone);
    }

    public bool PlayerIsInAnyZone()
    {
        return currentPlayerZones.Count > 0;
    }

    public CameraZone GetCurrentZone()
    {
        if (!PlayerIsInAnyZone())
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
        HandleRegionText(currentZone.regionName, currentZone.isRestricted);
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
        CameraManager.Instance.BlendTo(camera);
        OnPlayerEnterZone?.Invoke(GetCurrentZone());
    }

    private void HandleRegionText(string regionName, bool isRestricted)
    {
        UIManager.Instance.regionText.DisplayText(regionName, isRestricted);
    }
}
