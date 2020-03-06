using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionManager : Singleton<RegionManager>
{
    [Header("Camera Zones")]
    public CameraZone diningArea;
    public CameraZone kitchen;
    public CameraZone hallway;
    public CameraZone birdiesRoom;

    [Header("Region Triggers")]
    public RegionTrigger finalHallwayDoor;
}
