using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : Interactable
{
    public GameObject pingPrefab;
    public float pingFloorOffset = 1.0f;
    private bool hasBeenAcquired = false;
    private bool hasHit = false;
    
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
        
        Vector3 hitPoint = other.GetContact(0).point;
        Vector3 hitNormal = other.GetContact(0).normal;
        
        Instantiate(pingPrefab, hitPoint + (pingFloorOffset * hitNormal), 
            Quaternion.LookRotation(-hitNormal));
        hasHit = true;
    }
}
