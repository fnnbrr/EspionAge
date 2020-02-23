using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class MissionNPC
{
    public GameObject missionPrefab;

    // Type of conversations an NPC can have based on if it is before, during or after a
    public Conversation beforeConvo;
    public Conversation duringConvo;
    public Conversation afterConvo;
}

public class NPCInteractable : DialogueInteractable
{
    public List<Conversation> defaultConvos;

    public List<MissionNPC> missionsOffered;
    private MissionNPC currentMissionNPC;                            // Current Mission given by this NPC (should only be 1 per NPC)
    private AMission startedMission;                                 // started mission needed to end mission

    protected GameObject targetObject;


    public override void OnInteract()
    {
        if (!isConversing)
        {
            // Load the conversation of the NPC based on mission progress
            if (missionsOffered.Count == 0)
            {
                // Return a random default convo (Temporary until better system is set for default convos)
                conversation = defaultConvos[Random.Range(0, defaultConvos.Count)];
            }
            // NPC has mission to offer
            else
            {
                // Start mission
                if (startedMission == null)
                {
                    // Obtainment of mission and removal of offered mission
                    currentMissionNPC = missionsOffered[0];

                    // Start mission and store reference because needed to end mission
                    startedMission = MissionManager.Instance.StartMission(currentMissionNPC.missionPrefab);
                    ProgressManager.Instance.AddMission(startedMission);
                    startedMission.OnMissionComplete += HandleOnMissionComplete;
                    startedMission.OnMissionReset += HandleOnMissionReset;

                    conversation = currentMissionNPC.beforeConvo;

                    Debug.Log("NPC: beforeConvo");

                }
                // Mission started but not completed
                else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Started)
                {
                    conversation = currentMissionNPC.duringConvo;
                    Debug.Log("NPC: duringConvo");
                }
                // Objective of mission has been completed but now talking to NPC to close mission
                else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Completed)
                {
                    // Ends current mission with NPC
                    MissionManager.Instance.EndMission(startedMission);
                    ProgressManager.Instance.UpdateMissionStatus(startedMission, MissionStatusCode.Closed);

                    // Unsubscribe
                    startedMission.OnMissionComplete -= HandleOnMissionComplete;
                    startedMission.OnMissionReset -= HandleOnMissionReset;
                    startedMission = null;
                    missionsOffered.RemoveAt(0);

                    conversation = currentMissionNPC.afterConvo;

                    Debug.Log("NPC: afterConvo");
                }
                else
                {
                    Debug.LogError("Should not be getting to here");
                    return;
                }
            }
            NPCFacePlayer();
        }
        
        base.OnInteract();
    }

    // To be called in another class
    public void StartAutoplayInteraction()
    {
        TriggerAutoplay();
        OnInteract();
    }


    private void HandleOnMissionReset()
    {
        ProgressManager.Instance.UpdateMissionStatus(startedMission, MissionStatusCode.Started);
    }

    private void HandleOnMissionComplete()
    {
        ProgressManager.Instance.UpdateMissionStatus(startedMission, MissionStatusCode.Completed);
        Debug.Log("Objective Complete");
    }


    // Makes player face interactable when they are interacted with
    public void NPCFacePlayer()
    {
        Vector3 dirToFace = player.transform.position - transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        StartCoroutine(RotateAnimation(gameObject, rotation, player.GetComponent<PlayerController>().turnSpeed));
    }
}