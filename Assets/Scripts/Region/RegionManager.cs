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
    [BoxGroup("Camera Zones")] public CameraZone brutusOffice;

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
    private Dictionary<GameObject, List<CameraZone>> trackedObjectZones;

    public delegate void EnterZoneAction(CameraZone zone);
    public delegate void ExitZoneAction(CameraZone zone);
    public event EnterZoneAction OnPlayerEnterZone;
    public event ExitZoneAction OnPlayerExitZone;

    // Regions
    private List<RegionTrigger> currentPlayerRegions;
    private Dictionary<GameObject, List<RegionTrigger>> trackedObjectRegions;

    public delegate void EnterRegionAction(RegionTrigger region);
    public delegate void ExitRegionAction(RegionTrigger region);
    public event EnterRegionAction OnPlayerEnterRegion;
    public event ExitRegionAction OnPlayerExitRegion;

    private void Awake()
    {
        currentPlayerZones = new List<CameraZone>();
        trackedObjectZones = new Dictionary<GameObject, List<CameraZone>>();

        currentPlayerRegions = new List<RegionTrigger>();
        trackedObjectRegions = new Dictionary<GameObject, List<RegionTrigger>>();
    }

    public bool PlayerIsInZone(CameraZone zone)
    {
        return currentPlayerZones.Contains(zone);
    }

    public bool PlayerIsInAnyZone()
    {
        return currentPlayerZones.Count > 0;
    }

    public CameraZone GetPlayerCurrentZone()
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

    public void RegisterTrackedObject(GameObject trackedObject)
    {
        if (!trackedObjectZones.ContainsKey(trackedObject))
        {
            trackedObjectZones.Add(trackedObject, new List<CameraZone>());
        }
        if (!trackedObjectRegions.ContainsKey(trackedObject))
        {
            trackedObjectRegions.Add(trackedObject, new List<RegionTrigger>());
        }
    }

    public void UnregisterTrackedObject(GameObject trackedObject)
    {
        if (trackedObjectZones.ContainsKey(trackedObject))
        {
            trackedObjectZones.Remove(trackedObject);
        }
        if (trackedObjectRegions.ContainsKey(trackedObject))
        {
            trackedObjectRegions.Remove(trackedObject);
        }
    }

    public bool IsInZone(GameObject trackedObject, CameraZone zone)
    {
        return trackedObjectZones.ContainsKey(trackedObject) && trackedObjectZones[trackedObject].Contains(zone);
    }

    public void ReportTrackedObjectEnterZone(GameObject trackedObject, CameraZone zone)
    {
        if (trackedObjectZones.ContainsKey(trackedObject))
        {
            Debug.Log($"Tracked object {trackedObject.name} entered zone {zone.name}");
            trackedObjectZones[trackedObject].Add(zone);
        }
    }

    public void ReportTrackedObjectExitZone(GameObject trackedObject, CameraZone zone)
    {
        if (trackedObjectZones.ContainsKey(trackedObject))
        {
            Debug.Log($"Tracked object {trackedObject.name} exited zone {zone.name}");
            trackedObjectZones[trackedObject].Remove(zone);
        }
    }

    private void HandleCurrentZone()
    {
        CameraZone currentZone = GetPlayerCurrentZone();
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
        OnPlayerEnterZone?.Invoke(GetPlayerCurrentZone());
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
        OnPlayerEnterRegion?.Invoke(region);
    }

    public void ReportPlayerExitRegion(RegionTrigger region)
    {
        Debug.Log($"Exited region {region.name}");
        currentPlayerRegions.Remove(region);
        OnPlayerExitRegion?.Invoke(region);
    }

    public bool IsInRegion(GameObject trackedObject, RegionTrigger region)
    {
        return trackedObjectRegions.ContainsKey(trackedObject) && trackedObjectRegions[trackedObject].Contains(region);
    }

    public void ReportTrackedObjectEnterRegion(GameObject trackedObject, RegionTrigger region)
    {
        if (trackedObjectRegions.ContainsKey(trackedObject))
        {
            Debug.Log($"Tracked object {trackedObject.name} entered region {region.name}");
            trackedObjectRegions[trackedObject].Add(region);
        }
    }

    public void ReportTrackedObjectExitRegion(GameObject trackedObject, RegionTrigger region)
    {
        if (trackedObjectRegions.ContainsKey(trackedObject))
        {
            Debug.Log($"Tracked object {trackedObject.name} exited region {region.name}");
            trackedObjectRegions[trackedObject].Remove(region);
        }
    }
}
