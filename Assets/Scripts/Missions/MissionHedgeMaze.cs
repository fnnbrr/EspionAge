using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MissionHedgeMaze : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    public GameObject brutusOffice;
    public MissionObject brutusResponser;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        // Start the code-red cutscene

        brutusOffice.SetActive(false);
        brutusResponser.spawnedInstance = MissionManager.Instance.SpawnMissionObject(brutusResponser);
    }

    protected override void Cleanup()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        brutusOffice.SetActive(true);

        if (MissionManager.Instance) MissionManager.Instance.DestroyMissionObject(brutusResponser);
    }
}
