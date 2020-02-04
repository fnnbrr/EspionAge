using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : Interactable
{
    public override void OnInteract(GameObject birdie)
    {
        base.OnInteract(birdie);

        PlayerManager playerManager = Utils.GetRequiredComponent<PlayerManager>(birdie);
        playerManager.AddThrowable(gameObject);

        // TODO: Play object pickup sound effect here?
    }
}
