using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestArea : MonoBehaviour
{
    private PlayerManager GetPlayerManager(GameObject o)
    {
        return Utils.GetRequiredComponent<PlayerManager>(o, $"Object with tag '{Constants.TAG_PLAYER}' found without 'PlayerManager' component.");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Tell the player that they can rest
            PlayerManager playerManager = GetPlayerManager(other.gameObject);
            //playerManager.CanRest = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Tell the player they can no longer rest
            PlayerManager playerManager = GetPlayerManager(other.gameObject);
            //playerManager.CanRest = false;
        }
    }
}
