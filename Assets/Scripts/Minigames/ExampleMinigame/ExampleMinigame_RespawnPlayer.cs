using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMinigame_RespawnPlayer : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            other.transform.position = respawnPoint.position;
        }
    }
}
