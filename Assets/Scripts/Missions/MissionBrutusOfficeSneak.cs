using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MissionBrutusOfficeSneak : AMission
{
    public PlayableDirector onEnterCutscene;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionBrutusOfficeSneak");

        RegionManager.Instance.OnPlayerEnterZone += OnPlayerEnterOffice;
    }

    private void OnPlayerEnterOffice(CameraZone zone)
    {
        if (zone == RegionManager.Instance.brutusOffice)
        {
            RegionManager.Instance.OnPlayerEnterZone -= OnPlayerEnterOffice;

            onEnterCutscene.Play();
            StartCoroutine(DisablePlayerMovementDuringCutscene(onEnterCutscene));
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
    }
}
