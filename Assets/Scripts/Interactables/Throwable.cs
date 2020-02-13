using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : Interactable
{
    public override void OnInteract()
    {
        base.OnInteract();

        PlayerManager playerManager = Utils.GetRequiredComponent<PlayerManager>(player);
        playerManager.AddThrowable(gameObject);

        // TODO: Play object pickup sound effect here?
    }
}
