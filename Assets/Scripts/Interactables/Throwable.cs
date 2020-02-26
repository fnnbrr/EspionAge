using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NoisePing))]
public class Throwable : Interactable
{
    private bool hasBeenAcquired = false;
    private bool hasHit = false;
    private NoisePing noisePing;

    protected override void Start()
    {
        base.Start();
        noisePing = gameObject.GetComponent<NoisePing>();
    }

    public override void OnInteract()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;

        if (!hasBeenAcquired)
        {
            base.OnInteract();

            PlayerManager playerManager = Utils.GetRequiredComponent<PlayerManager>(player);
            playerManager.AddThrowable(gameObject);
            hasBeenAcquired = true;
            enableInteract = false;  // the player should not be able to interact with this object anymore
            
            // TODO: Play object pickup sound effect here?
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!hasBeenAcquired || hasHit) return;

        noisePing.SpawnNoisePing(other);
        hasHit = true;
    }
}
