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

    public bool testStopMission = false;
    public float afterSeconds;

    private IMission startedMission;

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
            startedMission = MissionManager.Instance.StartMission(missionPrefab);
        }
        ////////////////////////////////////////////////////
    }

    private void Update()
    {
        ////////////////////////////////////////////////////
        // TOOD: REMOVE ONCE DIALOGUE IS IMPLEMENTED
        // - This is here instead of the MissionManager to test calling this from another class
        if (startedMission != null && testStopMission && missionPrefab && Time.time >= afterSeconds)
        {
            MissionManager.Instance.EndMission(startedMission);

            startedMission = null;
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
