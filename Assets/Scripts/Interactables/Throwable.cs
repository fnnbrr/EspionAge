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

    private void Start()
    {
        noisePing = gameObject.GetComponent<NoisePing>();
    }

    public override void OnInteract()
    {
        base.OnInteract();

        PlayerManager playerManager = Utils.GetRequiredComponent<PlayerManager>(player);
        playerManager.AddThrowable(gameObject);
        hasBeenAcquired = true;

        // TODO: Play object pickup sound effect here?
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!hasBeenAcquired || hasHit) return;

        noisePing.SpawnNoisePing(other);
        hasHit = true;
    }
}
