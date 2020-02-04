using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameObject player;
    public bool enableFog = false;

    private PlayerManager playerManager;
    private PlayerController playerController;

    private void Start()
    {
        if (!player)
        {
            Utils.LogErrorAndStopPlayMode("GameManager expects a reference to a main player GameObject!");
        }

        playerManager = player.GetComponent<PlayerManager>();
        playerController = player.GetComponent<PlayerController>();
    }

    public Transform GetPlayerTransform()
    {
        return player.transform;
    }

    public PlayerManager GetPlayerManager()
    {
        return playerManager;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }
}
