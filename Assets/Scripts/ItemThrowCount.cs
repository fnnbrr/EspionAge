using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemThrowCount : MonoBehaviour
{
    private int numItemsHeld = 0;
    ThrowController throwController;
    TextMeshProUGUI text;

    void Start()
    {
        throwController = GameManager.Instance.GetPlayerManager();
        if(throwController == null)
        {
            Utils.LogErrorAndStopPlayMode("Player manager does not exist");
        }
        throwController.OnThrow += HandleOnThrewBottle;
        throwController.OnPickup += HandleOnPickup;
        throwController.OnThrowableReset += HandleResetThrowableCount;

        text = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this, "Throwable Object text is null!");
        DisplayUpdate();
    }

    private void HandleResetThrowableCount()
    {
        numItemsHeld = 0;
        DisplayUpdate();
    }

    void HandleOnPickup(GameObject source)
    {
        numItemsHeld += 1;
        DisplayUpdate();
    }

    void HandleOnThrewBottle(Interactable source)
    {
        numItemsHeld -= 1;
        DisplayUpdate();
    }

    void DisplayUpdate()
    {
        text.SetText("x" + numItemsHeld.ToString());
    }
}
