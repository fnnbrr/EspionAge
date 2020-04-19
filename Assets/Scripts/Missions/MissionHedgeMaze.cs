using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using NPCs;
using NaughtyAttributes;
using NPCs.Components;

public class MissionHedgeMaze : AMission
{
    [Header("Respawning")]
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    [Header("Cutscenes")]
    public PlayableDirector codeRedCutscene;
    public PlayableDirector finalSequenceCutscene;
    public Vector3 finalSequenceBrutusPosition;
    public Vector3 finalSequenceBrutusRotation;
    public PlayableDirector gameOverCutscene;

    [Header("Frank the Nurse")]
    public GameObject frankBeforeMission;
    public Vector3 frankPosition;
    public Vector3 frankRotation;
    public Conversation finalConversation;

    [Header("Escape Window")]
    public GameObject escapeWindow;
    public Vector3 windowPosition;
    public Vector3 windowRotation;
    public GameObject escapeTable;
    public Vector3 tablePosition;
    public Vector3 tableRotation;
    public GameObject tableRadio;
    public Vector3 radioPosition;
    public Vector3 radioRotation;

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

    [Header("FMODManager")]
    public FMODManager fmod;

    private bool isRestarting;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        StartCoroutine(StartMission());

        // TODO: flip this logic and make the tutorial just set it to be one-way temporarily
        RegionManager.Instance.nurseRoomDoor.SetMinLimit(-90f);  // allow to open fully now (both ways)
        RegionManager.Instance.nurseRoomDoor.shakeCameraOnClose = false;

        DialogueManager.Instance.OnFinishConversation += WaitForFinalConversationEnd;
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
        npcInteractable.ResetAllConversations();
        npcInteractable.defaultConvos = new List<Conversation>() { finalConversation };
        npcInteractable.conversation = finalConversation;

        // Perfect positioning of all the nurse room escape objects
        escapeTable.transform.position = tablePosition;
        escapeTable.transform.rotation = Quaternion.Euler(tableRotation);
        tableRadio.transform.position = radioPosition;
        tableRadio.transform.rotation = Quaternion.Euler(radioRotation);
    }

    private void WaitForFinalConversationEnd(Conversation conversation)
    {
        if (conversation != finalConversation) return;

        DialogueManager.Instance.OnFinishConversation -= WaitForFinalConversationEnd;

        TurnOffAllBarks();

        StartCoroutine(StartFinalSequence());
    }


    private void TurnOffAllBarks()
    {
        foreach (BasicNurse enemy in spawnedHedgeMazeEnemies)
        {
            enemy.gameObject.GetComponent<NPCReactiveBark>().TurnOffBark();
        }

        foreach (NPCReactiveBark enemy in spawnedSurroundingEnemies.GetComponentsInChildren<NPCReactiveBark>())
        {
            enemy.TurnOffBark();
        }

        brutusResponderAI.GetComponent<NPCReactiveBark>().TurnOffBark();
    }

    private IEnumerator StartFinalSequence()
    {
        // Hide speaker UI for Frank from the cutscene
        Utils.GetRequiredComponent<NPCInteractable>(frankBeforeMission).enabled = false;
        Utils.GetRequiredComponent<SpeakerUI>(frankBeforeMission).Hide();

        brutusResponderAI.agent.Warp(finalSequenceBrutusPosition);
        brutusResponderAI.transform.rotation = Quaternion.Euler(finalSequenceBrutusRotation);
        brutusResponderAI.ForceChasing();
        brutusResponderAI.agent.speed = 0.9f;

        // Now, we open the window
        escapeWindow.transform.position = windowPosition;
        escapeWindow.transform.rotation = Quaternion.Euler(windowRotation);

        yield return MissionManager.Instance.DisablePlayerMovementDuringCutscene(finalSequenceCutscene);

        // Force awakeness to max!
        UIManager.Instance.staminaBar.overrideValue = true;
        UIManager.Instance.staminaBar.overrideTo = StaminaBar.FILL_MAX;

        UIManager.Instance.CanPause = false;
        Time.timeScale = 0.5f;

        RegionManager.Instance.OnPlayerExitZone += WaitForPlayerToLeaveMap;
    }

    private void WaitForPlayerToLeaveMap(CameraZone zone)
    {
        if (RegionManager.Instance.GetPlayerCurrentZone() != null) return;
        RegionManager.Instance.OnPlayerExitZone -= WaitForPlayerToLeaveMap;

        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        Time.timeScale = 0f;
        yield return MissionManager.Instance.DisablePlayerMovementDuringCutscene(gameOverCutscene);
        Time.timeScale = 1f;

        yield return UIManager.Instance.credits.Show(true);

        fmod.KillAllAudio();
        SceneManager.LoadScene(Constants.SCENE_MAINMENU);
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
        
        Chaser.ResetChaserCount();

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
