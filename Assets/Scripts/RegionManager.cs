using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

public class RegionManager : Singleton<RegionManager>
{
    [HorizontalLine(height: 1)] 
    [BoxGroup("Camera Zones")] public CameraZone diningArea;
    [BoxGroup("Camera Zones")] public CameraZone kitchen;
    [BoxGroup("Camera Zones")] public CameraZone hallway;
    [BoxGroup("Camera Zones")] public CameraZone nursesRoom;

    [HorizontalLine(height: 1)]
    [BoxGroup("Region Triggers")]                                       public RegionTrigger finalHallwayDoor;
    [Header("Nurse Room")]
    [BoxGroup("Region Triggers")] [Label("Birdies Bed / Window Area")]  public RegionTrigger nurseRoomBirdiesBedArea;
    [BoxGroup("Region Triggers")] [Label("Other Bed Area")]             public RegionTrigger nurseRoomOtherBedArea;
    [BoxGroup("Region Triggers")] [Label("Door Area")]                  public RegionTrigger nurseRoomDoorArea;

    [HorizontalLine(height: 1)]
    [BoxGroup("Doors")] public DoorBehaviour nurseRoomDoor;

    // Zones
    // Treating this list as a stack where the last element is considered "current" zone
    private List<CameraZone> currentPlayerZones;

    public delegate void EnterZoneAction(CameraZone zone);
    public delegate void ExitZoneAction(CameraZone zone);
    public event EnterZoneAction OnPlayerEnterZone;
    public event ExitZoneAction OnPlayerExitZone;

    // Regions
    private List<RegionTrigger> currentPlayerRegions;

    private void Awake()
    {
        currentPlayerZones = new List<CameraZone>();
        currentPlayerRegions = new List<RegionTrigger>();
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
        HandleZoneText(currentZone.regionName, currentZone.isRestricted);
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

    private void HandleZoneText(string zoneName, bool isRestricted)
    {
        UIManager.Instance.zoneText.DisplayText(zoneName, isRestricted);
    }

    public bool PlayerIsInRegion(RegionTrigger region)
    {
        return currentPlayerRegions.Contains(region);
    }

    public void ReportPlayerEnterRegion(RegionTrigger region)
    {
        Debug.Log($"Entered region {region.name}");
        currentPlayerRegions.Add(region);
    }

    public void ReportPlayerExitRegion(RegionTrigger region)
    {
        Debug.Log($"Exited region {region.name}");
        currentPlayerRegions.Remove(region);
    }
}
