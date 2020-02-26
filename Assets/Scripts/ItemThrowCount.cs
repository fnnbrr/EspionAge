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
            Debug.LogError("Player manager does not exist");
        }

        playerManager.OnThrow += HandleOnThrewBottle;
        playerManager.OnPickup += OnHandlePickup;

        text = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this);

        if(text == null)
        {
            Debug.LogError("Throwable Object text is null");
        }
        text.SetText("x" + numItemsHeld.ToString());
    }

    void OnHandlePickup(GameObject source)
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
