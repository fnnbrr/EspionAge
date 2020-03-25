using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NoisePinger))]
public class Throwable : Interactable
{
    private bool hasHit = false;
    private NoisePinger noisePing;

    [FMODUnity.EventRef]
    public string throwableSFX;
    [FMODUnity.EventRef]
    public string pickUpSFX;

    protected override void Start()
    {
        base.Start();
        noisePing = gameObject.GetComponent<NoisePinger>();
    }

    public override void OnInteract()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;

        if (enableInteract)
        {
            base.OnInteract();
            
            enableInteract = false;
            GameManager.Instance.GetThrowController().AddThrowable(gameObject);
            
            FMODUnity.RuntimeManager.PlayOneShot(pickUpSFX, transform.position);
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        FMODUnity.RuntimeManager.PlayOneShot(throwableSFX, transform.position);
        if (enableInteract || hasHit) return;

        noisePing.SpawnNoisePing(other);
        hasHit = true;
    }
}
