using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODRadio : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string Music;
    FMOD.Studio.EventInstance musicEv;
    public bool isPlaying;

    private Interactable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = Utils.GetRequiredComponent<Interactable>(this);
        interactable.OnInteractEnd += ChangeStation;
        musicEv = FMODUnity.RuntimeManager.CreateInstance(Music);
        musicEv.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position)); 
        musicEv.start();
    }

    public void ChangeStation(Interactable source)
    {
        if (isPlaying)
        {
            musicEv.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicEv.start();
        }
    }
}
