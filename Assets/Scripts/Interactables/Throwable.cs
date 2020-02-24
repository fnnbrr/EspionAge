using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : Interactable
{
    private bool pickedUp = false;

    public override void OnInteract()
    {
        if (!pickedUp)
        {
            base.OnInteract();

            PlayerManager playerManager = Utils.GetRequiredComponent<PlayerManager>(player);
            playerManager.AddThrowable(gameObject);

            // TODO: Play object pickup sound effect here?

            pickedUp = true;
            enableInteract = false;  // the player should not be able to interact with this object anymore
        }
    }
}
