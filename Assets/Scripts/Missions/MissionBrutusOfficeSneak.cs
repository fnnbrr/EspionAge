using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class MissionBrutusOfficeSneak : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;
    public AIBrutusOffice brutusAI;
    public PlayableDirector onEnterCutscene;
    public MissionObject papersInteractable;

    private Interactable papersInteractableComponent;
    private bool isRestarting = false;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionBrutusOfficeSneak");

        // Instantiate mission-specific objects
        papersInteractable.spawnedInstance = MissionManager.Instance.SpawnMissionObject(papersInteractable);
        papersInteractableComponent = Utils.GetRequiredComponent<Interactable>(papersInteractable.spawnedInstance);
        papersInteractableComponent.OnInteractEnd += HandlePapersCollected;

        RegionManager.Instance.brutusOfficeDoor.SetLocked(false);
        RegionManager.Instance.OnPlayerEnterZone += HandlePlayerEnterOffice;

        brutusAI.enemy.OnCollideWithPlayer += HandlePlayerCaught;

        ObjectiveList.Instance.SlideOutObjectTextForSeconds(5f);
    }

    private void HandlePapersCollected(Interactable source)
    {
        papersInteractableComponent.OnInteractEnd -= HandlePapersCollected;

        Destroy(papersInteractable.spawnedInstance);

        MissionManager.Instance.CompleteMissionObjective(MissionsEnum.BrutusOfficeSneak);
        AlertMissionComplete();
    }

    private void HandlePlayerCaught()
    {
        if (isRestarting) return;

        isRestarting = true;
        AlertMissionReset();

        StartCoroutine(HandleMissionReset());
    }

    private IEnumerator HandleMissionReset()
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;

        yield return UIManager.Instance.FadeOut();

        brutusAI.HardResetToIdle();

        UIManager.Instance.staminaBar.ResetAwakeness();
        GameManager.Instance.GetThrowController().ResetThrowables();

        GameManager.Instance.GetPlayerTransform().position = respawnPosition;
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(respawnRotation);
        GameManager.Instance.GetMovementController().ResetVelocity();

        Cleanup();
        Initialize();
        RegionManager.Instance.OnPlayerEnterZone -= HandlePlayerEnterOffice;

        yield return UIManager.Instance.FadeIn();

        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
        isRestarting = false;
    }

    private void HandlePlayerEnterOffice(CameraZone zone)
    {
        if (zone == RegionManager.Instance.brutusOffice)
        {
            RegionManager.Instance.OnPlayerEnterZone -= HandlePlayerEnterOffice;

            if (!GameManager.Instance.skipSettings.allRealtimeCutscenes)
            {
                onEnterCutscene.Play();
                StartCoroutine(DisablePlayerMovementDuringCutscene(onEnterCutscene));
            }
        }
    }

    private IEnumerator DisablePlayerMovementDuringCutscene(PlayableDirector cutsceneDirector)
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        while(cutsceneDirector.state == PlayState.Playing)
        {
            yield return null;
        }
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
    }

    protected override void Cleanup()
    {
        Debug.Log("Cleaning up mission: MissionBrutusOfficeSneak");

        if (MissionManager.Instance) MissionManager.Instance.DestroyMissionObject(papersInteractable);

        papersInteractableComponent.OnInteractEnd -= HandlePapersCollected;
        if (RegionManager.Instance) RegionManager.Instance.OnPlayerEnterZone -= HandlePlayerEnterOffice;
        brutusAI.enemy.OnCollideWithPlayer -= HandlePlayerCaught;
    }
}
