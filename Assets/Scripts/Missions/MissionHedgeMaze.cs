using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using NPCs;
using NaughtyAttributes;

public class MissionHedgeMaze : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    public PlayableDirector codeRedCutscene;

    public GameObject frankBeforeMission;
    public MissionObject frankNurseRoom;

    public GameObject brutusOffice;
    public MissionObject brutusResponser;
    private BrutusResponder brutusResponderAI;

    public GameObject surroundingEnemiesPrefab;
    private GameObject spawnedSurroundingEnemies;

    [ReorderableList]
    public List<MissionEnemy> hedgeMazeEnemies;
    private List<BasicNurse> spawnedHedgeMazeEnemies;

    private bool isRestarting;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        StartCoroutine(StartMission());
    }

    private IEnumerator StartMission()
    {
        yield return MissionManager.Instance.DisablePlayerMovementDuringCutscene(codeRedCutscene);

        brutusOffice.SetActive(false);

        if (GameManager.Instance.skipSettings.allRealtimeCutscenes)
        {
            // since thse are triggered via a signal in the cutscene itself
            SpawnSurroundingEnemies();
            DespawnFrank();
        }
        SpawnBrutusResponser();
    }

    private void SpawnBrutusResponser()
    {
        // Spawn and set up Brutus Responder AI
        brutusResponser.spawnedInstance = MissionManager.Instance.SpawnMissionObject(brutusResponser);
        brutusResponderAI = Utils.GetRequiredComponent<BrutusResponder>(brutusResponser.spawnedInstance);
        brutusResponderAI.enemy.OnCollideWithPlayer += RestartMission;
    }

    // Called by Signal Emitter in Unity Timeline
    public void SpawnSurroundingEnemies()
    {
        spawnedSurroundingEnemies = Instantiate(surroundingEnemiesPrefab);
        foreach (BasicNurse nurseAI in spawnedSurroundingEnemies.GetComponentsInChildren<BasicNurse>())
        {
            nurseAI.enemy.OnCollideWithPlayer += RestartMission;
        }

        spawnedHedgeMazeEnemies = MissionManager.Instance.SpawnEnemyNurses(hedgeMazeEnemies, RestartMission);
    }

    // Called by Signal Emitter in Unity Timeline
    public void DespawnFrank()
    {
        frankBeforeMission.SetActive(false);
        frankNurseRoom.spawnedInstance = MissionManager.Instance.SpawnMissionObject(frankNurseRoom);
    }

    private void RestartMission()
    {
        if (!isRestarting)
        {
            isRestarting = true;
            StartCoroutine(DoRestartMission());
        }
    }

    private IEnumerator DoRestartMission()
    {
        yield return UIManager.Instance.FadeOut();

        GameManager.Instance.GetPlayerTransform().position = respawnPosition;
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(respawnRotation);

        DestroyAllEnemies();

        SpawnBrutusResponser();
        SpawnSurroundingEnemies();

        MissionManager.Instance.RestartMission(MissionsEnum.HedgeMaze);

        yield return UIManager.Instance.FadeIn();
        isRestarting = false;
    }

    private void DestroyAllEnemies()
    {
        Destroy(spawnedSurroundingEnemies);
        if (MissionManager.Instance) MissionManager.Instance.DestroyEnemyNurses(spawnedHedgeMazeEnemies);
        spawnedHedgeMazeEnemies.Clear();

        if (MissionManager.Instance) MissionManager.Instance.DestroyMissionObject(brutusResponser);
    }

    protected override void Cleanup()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        if (brutusOffice) brutusOffice.SetActive(true);
        if (frankBeforeMission) frankBeforeMission.SetActive(true);

        if (MissionManager.Instance) MissionManager.Instance.DestroyMissionObject(frankNurseRoom);

        DestroyAllEnemies();
    }
}
