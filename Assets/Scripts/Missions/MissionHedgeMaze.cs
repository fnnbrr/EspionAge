using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using NPCs;
using NaughtyAttributes;

public class MissionHedgeMaze : AMission
{
    [Header("Respawning")]
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    [Header("Cutscene - Code Red")]
    public PlayableDirector codeRedCutscene;

    [Header("Frank the Nurse")]
    public GameObject frankBeforeMission;
    public Vector3 frankPosition;
    public Vector3 frankRotation;
    public Conversation finalConversation;

    [Header("Escape Window")]
    public GameObject escapeWindow;
    public Vector3 windowPosition;
    public Vector3 windowRotation;

    [Header("Brutus")]
    public GameObject brutusOffice;
    public MissionObject brutusResponser;
    private BrutusResponder brutusResponderAI;

    [Header("Enemies")]
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
            SetupNurseRoom();
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
    public void SetupNurseRoom()
    {
        frankBeforeMission.transform.position = frankPosition;
        frankBeforeMission.transform.rotation = Quaternion.Euler(frankRotation);
        NPCInteractable npcInteractable = Utils.GetRequiredComponent<NPCInteractable>(frankBeforeMission);
        npcInteractable.defaultConvos = new List<Conversation>() { finalConversation };
        npcInteractable.missionsOffered.Clear();

        escapeWindow.transform.position = windowPosition;
        escapeWindow.transform.rotation = Quaternion.Euler(windowRotation);
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

        DestroyAllEnemies();
    }
}
