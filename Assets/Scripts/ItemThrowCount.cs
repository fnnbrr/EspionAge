using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemThrowCount : MonoBehaviour
{
    private int numItemsHeld = 0;
    PlayerManager playerManager;
    TextMeshProUGUI text;

    void Start()
    {
        playerManager = GameManager.Instance.GetPlayerManager();
        if(playerManager == null)
        {
            Utils.LogErrorAndStopPlayMode("Player manager does not exist");
        }
        playerManager.OnThrow += HandleOnThrewBottle;
        playerManager.OnPickup += HandleOnPickup;
        playerManager.OnThrowableReset += HandleResetThrowableCount;

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
