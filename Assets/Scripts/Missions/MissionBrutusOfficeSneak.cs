using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MissionBrutusOfficeSneak : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;
    public PlayableDirector onEnterCutscene;
    public MissionObject papersInteractable;

    [Header("TEMP DEBUG")]
    public bool restart = false;

    private Interactable papersInteractableComponent;

    ///////////////////////////
    // TEMP UPDATE
    private void Update()
    {
        if (restart)
        {
            restart = false;
            HandlePlayerCaught();
        }
    }
    ///////////////////////////

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionBrutusOfficeSneak");

        // Instantiate mission-specific objects
        papersInteractable.spawnedInstance = MissionManager.Instance.SpawnMissionObject(papersInteractable);
        papersInteractableComponent = Utils.GetRequiredComponent<Interactable>(papersInteractable.spawnedInstance);
        papersInteractableComponent.OnInteractEnd += HandlePapersCollected;

        RegionManager.Instance.OnPlayerEnterZone += HandlePlayerEnterOffice;

        ObjectiveList.Instance.SlideOutObjectTextForSeconds(5f);
    }

    private void HandlePapersCollected(Interactable source)
    {
        papersInteractableComponent.OnInteractEnd -= HandlePapersCollected;

        Destroy(papersInteractable.spawnedInstance);

        MissionManager.Instance.CompleteMissionObjective(MissionsEnum.BrutusOfficeSneak);
        AlertMissionComplete();
    }

    // TODO: Hook this up with Brutus OnCaught
    private void HandlePlayerCaught()
    {
        AlertMissionReset();

        StartCoroutine(HandleMissionReset());
    }

    private IEnumerator HandleMissionReset()
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;

        yield return UIManager.Instance.FadeOut();

        Cleanup();
        Initialize();

        GameManager.Instance.GetPlayerTransform().position = respawnPosition;
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(respawnRotation);
        GameManager.Instance.GetMovementController().ResetVelocity();

        yield return UIManager.Instance.FadeIn();

        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
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
    }
}
