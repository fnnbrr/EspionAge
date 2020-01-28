using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject player;

    private PlayerManager playerManager;
    private PlayerController playerController;

    // TODO: Remove below once Dialogue + Triggering Mission start is implemented
    [Header("Remove below once we have dialogue")]
    public bool testStartMission = false;
    public GameObject missionPrefab;

    private void Start()
    {
        if (!player)
        {
            Utils.LogErrorAndStopPlayMode("GameManager expects a reference to a main player GameObject!");
        }

        playerManager = player.GetComponent<PlayerManager>();
        playerController = player.GetComponent<PlayerController>();

        ////////////////////////////////////////////////////
        // TOOD: REMOVE ONCE DIALOGUE IS IMPLEMENTED
        // - This is here instead of the MissionManager to test calling this from another class
        //   - (like Dialogue eventually will)
        if (testStartMission && missionPrefab)
        {
            MissionManager.Instance.StartMission(missionPrefab);
        }
        ////////////////////////////////////////////////////
    }

    public Transform GetPlayerTransform()
    {
        return player.transform;
    }

    public PlayerManager GetPlayerManager()
    {
        return playerManager;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }
}
