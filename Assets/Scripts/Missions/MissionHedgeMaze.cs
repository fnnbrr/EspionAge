using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MissionHedgeMaze : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    public PlayableDirector codeRedCutscene;

    public GameObject brutusOffice;
    public MissionObject brutusResponser;

    public GameObject surroundingEnemiesPrefab;
    private GameObject spawnedSurroundingEnemies;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        // Start the code-red cutscene

        //brutusResponser.spawnedInstance = MissionManager.Instance.SpawnMissionObject(brutusResponser);

        StartCoroutine(StartMission());
    }

    private IEnumerator StartMission()
    {
        codeRedCutscene.Play();
        yield return MissionManager.Instance.DisablePlayerMovementDuringCutscene(codeRedCutscene);
        brutusOffice.SetActive(false);
    }

    // Called by Signal Emitter in Unity Timeline
    public void SpawnSurroundingEnemies()
    {
        spawnedSurroundingEnemies = Instantiate(surroundingEnemiesPrefab);
    }

    protected override void Cleanup()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        if (brutusOffice) brutusOffice.SetActive(true);

        Destroy(spawnedSurroundingEnemies);

        //if (MissionManager.Instance) MissionManager.Instance.DestroyMissionObject(brutusResponser);
    }
}
