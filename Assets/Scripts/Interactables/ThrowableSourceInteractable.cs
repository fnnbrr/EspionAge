using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableSourceInteractable : Interactable
{
    public GameObject throwablePrefab;

    public override void OnInteract()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;

        GameObject instantiatedThrowable = Instantiate(throwablePrefab);
        Throwable throwable = Utils.GetRequiredComponent<Throwable>(instantiatedThrowable);
        throwable.OnInteract();

        base.OnInteract();
    }
}
