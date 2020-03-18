using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODRadio : MonoBehaviour
{
    public FMODUnity.StudioEventEmitter musicEv;
    public bool isPlaying;

    private Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = Utils.GetRequiredComponent<Interactable>(this);
        interactable.OnInteractEnd += ChangeStation;
        musicEv = GetComponent<FMODUnity.StudioEventEmitter>();
        musicEv.Play();
    }

    public void ChangeStation(Interactable source)
    {
        if (isPlaying)
        {
            musicEv.Stop();
            musicEv.Play();
        }
    }
}
