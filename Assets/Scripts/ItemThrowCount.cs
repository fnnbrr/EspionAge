using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemThrowCount : MonoBehaviour
{
    private int numItemsHeld = 0;
    private Animator rootAnim;
    private ThrowController throwController;
    private TextMeshProUGUI text;

    void Start()
    {
        rootAnim = Utils.GetRequiredComponent<Animator>(this);

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
            GameEventManager.Instance.SubscribeToEvent(GameEventManager.GameEvent.HasThrownSomething, OnFirstPickUp);
        }
    }

    private void ShowImage()
    {
        rootAnim.SetBool(Constants.ANIMATION_PILLBOTTLE_DISPLAYING, true);
    }

    private void HideImage()
    {
        rootAnim.SetBool(Constants.ANIMATION_PILLBOTTLE_DISPLAYING, false);
    }

    private void OnFirstPickUp(bool status)
    {
        if (status)
        {
            ShowImage();
        }
        else
        {
            HideImage();
        }

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
