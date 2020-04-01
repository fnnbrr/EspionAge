using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MissionHedgeMaze : AMission
{
    public Vector3 respawnPosition;
    public Vector3 respawnRotation;

    protected override void Initialize()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");

        // Start the code-red cutscene
    }

    protected override void Cleanup()
    {
        Debug.Log("Starting mission: MissionHedgeMaze");
    }
}
