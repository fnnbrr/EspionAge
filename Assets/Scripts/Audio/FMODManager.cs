using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODManager : MonoBehaviour
{
    [Header("Global Audio Events")]

    [EventRef]
    public string siren;
    [EventRef]
    public string stinger;
    [EventRef]
    public string chaseMusic;
    [EventRef]
    public string creditsMusic;
    [EventRef]
    public string brutus;
 

    [Header("Global Audio Parameters")]

    [ParamRef]
    public string pnoVolume;
    [ParamRef]
    public string startChase;
    [ParamRef]
    public string endChase;
    [ParamRef]
    public string killAudio;

    private FMOD.Studio.EventInstance ChaseMusic;
    private FMOD.Studio.EventInstance CreditsMusic;
    private FMOD.Studio.EventInstance Siren;

    void Start()
    {
        ChaseMusic = RuntimeManager.CreateInstance(chaseMusic);
        CreditsMusic = RuntimeManager.CreateInstance(creditsMusic);
        Siren = RuntimeManager.CreateInstance(siren);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SirenSounds()
    {
        Siren.start();
    }

    public void Stinger()
    {
        RuntimeManager.StudioSystem.setParameterByName(pnoVolume, 0f);
        RuntimeManager.StudioSystem.setParameterByName(startChase, 0f);
        RuntimeManager.StudioSystem.setParameterByName(endChase, 0f);
        ChaseMusic.start();
    }

    public void EndCutscene()
    {
        RuntimeManager.StudioSystem.setParameterByName(endChase, 1f);
        RuntimeManager.StudioSystem.setParameterByName(killAudio, 1f);
    }

    public void Credits()
    {
        CreditsMusic.start();
    }

    public void BrutusOneShots()
    {
        RuntimeManager.PlayOneShot(brutus, transform.position);
    }

    public void KillAllAudio()
    {
        CreditsMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        CreditsMusic.release();
        ChaseMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ChaseMusic.release();
        Siren.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        Siren.release();
    }
}
