using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMinigame_EndMinigame : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            if (MinigameManager.Instance.IsInMinigame())
            {
                MinigameManager.Instance.UnloadCurrentMinigame(MinigameStaminaManager.Instance.GetCurrentStamina());
            } 
            else
            {
                Debug.Log("You're probably testing this minigame alone, so this means you reached the end! (if not, we have a problem...)");
            }
        }
    }
}
