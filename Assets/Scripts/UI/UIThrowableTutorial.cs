using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIThrowableTutorial : MonoBehaviour
{
    public GameObject root;

    private void Start()
    {
        SetActive(false);

        GameEventManager.Instance.SubscribeToEvent(GameEventManager.GameEvent.HasThrownSomething, OnFirstPickUp);
    }

    private void OnFirstPickUp(bool status)
    {
        SetActive(status);
        if (status)
        {
            GameManager.Instance.GetThrowController().OnThrow += HideTutorialAfterThrowing;
        }
    }

    private void HideTutorialAfterThrowing(Interactable obj)
    {
        GameManager.Instance.GetThrowController().OnThrow -= HideTutorialAfterThrowing;

        SetActive(false);
    }

    private void SetActive(bool active)
    {
        root.SetActive(active);
    }
}
