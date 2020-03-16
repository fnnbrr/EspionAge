using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODMissionComplete : MonoBehaviour
{
    [FMODUnity.ParamRef]
    public string parameter;
    public bool OnMissionComplete = false;

    private FMOD.Studio.PARAMETER_DESCRIPTION parameterDescription;
    private Interactable interactable;

    void Start()
    {
        interactable = Utils.GetRequiredComponent<Interactable>(this);
        interactable.OnInteractEnd += missionComplete;
        FMODUnity.RuntimeManager.StudioSystem.getParameterDescriptionByName(parameter, out parameterDescription);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isComplete)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameter, 1f);
            isComplete = true;
        }
    }

    private void OnMissionComplete(Interactable source)
    {
        if (isComplete)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(parameter, 0f);
            isComplete = false;
        }
    }
}
