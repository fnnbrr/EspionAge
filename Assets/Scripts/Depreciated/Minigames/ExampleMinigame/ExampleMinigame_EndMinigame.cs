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
                Debug.Log($"You would have gained {MinigameStaminaManager.Instance.GetCurrentStamina()} stamina!");
            }
        }
    }
}
