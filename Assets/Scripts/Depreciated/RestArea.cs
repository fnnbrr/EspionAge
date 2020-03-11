using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestArea : MonoBehaviour
{
    private ThrowController GetPlayerManager(GameObject o)
    {
        return Utils.GetRequiredComponent<ThrowController>(o, $"Object with tag '{Constants.TAG_PLAYER}' found without 'ThrowController' component.");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Tell the player that they can rest
            ThrowController throwController = GetPlayerManager(other.gameObject);
            //throwController.CanRest = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Tell the player they can no longer rest
            ThrowController throwController = GetPlayerManager(other.gameObject);
            //throwController.CanRest = false;
        }
    }
}
