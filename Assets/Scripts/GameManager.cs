using System;
using UnityEngine;

[System.Serializable]
public class SkipSettings
{
    public bool allTextCutscenes;
    public bool allRealtimeCutscenes;
}

public class GameManager : Singleton<GameManager>
{
    public GameObject player;

    [Header("Game Settings")]
    public bool enableGameStart = false;
    public SkipSettings skipSettings;

    private ThrowController throwController;
    private PlayerController playerController;
    private MovementController movementController;

    private void Awake()
    {
        if (!player)
        {
            Utils.LogErrorAndStopPlayMode("GameManager expects a reference to a main player GameObject!");
        }
        
        throwController = Utils.GetRequiredComponent<ThrowController>(player);
        playerController = Utils.GetRequiredComponent<PlayerController>(player);
        movementController = Utils.GetRequiredComponent<MovementController>(player);
    }

    private void Start()
    {
        if (enableGameStart)
        {
            GameStart();
        }
    }

    public Transform GetPlayerTransform()
    {
        return player.transform;
    }

    public ThrowController GetThrowController()
    {
        return throwController;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }
    
    public MovementController GetMovementController()
    {
        return movementController;
    }

    private void GameStart()
    {
        // Kind of hacky way to get around the issue of this script being initialized before every other script
        //  disable all cameras in CameraZones, and then only turn on the one in birdiesRoom
        foreach(CameraZone cameraZone in FindObjectsOfType<CameraZone>())
        {
            if (cameraZone.mainCamera)
            {
                cameraZone.mainCamera.gameObject.SetActive(false);
            }
        }
        RegionManager.Instance.birdiesRoom.mainCamera.gameObject.SetActive(true);
        MissionManager.Instance.StartMission(MissionsEnum.MissionTutorial);
    }
}
