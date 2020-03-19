using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemThrowCount : MonoBehaviour
{
    public GameObject imageRoot;

    private int numItemsHeld = 0;
    private ThrowController throwController;
    private TextMeshProUGUI text;

    void Start()
    {
        throwController = GameManager.Instance.GetThrowController();
        if(throwController == null)
        {
            Utils.LogErrorAndStopPlayMode("Player manager does not exist");
        }
        throwController.OnThrow += HandleOnThrewBottle;
        throwController.OnPickup += HandleOnPickup;
        throwController.OnThrowableReset += HandleResetThrowableCount;

        text = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this, "Throwable Object text is null!");
        DisplayUpdate();

        if (!GameEventManager.Instance.CheckEventStatus(GameEventManager.GameEvent.HasThrownSomething))
        {
            HideImage();
            throwController.OnPickup += WaitForFirstPickup;
        }
    }

    private void ShowImage()
    {
        imageRoot.SetActive(true);
    }

    private void HideImage()
    {
        imageRoot.SetActive(false);
    }

    private void WaitForFirstPickup(GameObject source)
    {
        throwController.OnPickup -= WaitForFirstPickup;

        ShowImage();
        GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.HasThrownSomething, true);
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
        text.SetText(numItemsHeld.ToString());
    }
}
