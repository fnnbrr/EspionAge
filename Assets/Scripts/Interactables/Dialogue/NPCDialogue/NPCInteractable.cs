using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class NPCMissionConvos
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

    public List<NPCMissionConvos> missionsOffered;
    private NPCMissionConvos currentMissionConvos = null;                            // Current Mission given by this NPC (should only be 1 per NPC)
    private AMission startedMission;                                 // started mission needed to end mission


    protected NavMeshAgent agent;
    protected GameObject targetObject;

    protected bool isFollowing = false;
    private Vector3 originPosition;

    protected override void Start()
    {
        base.Start();
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        originPosition = gameObject.transform.position;
        LoadConversation();
    }

    protected override void Update()
    {
        if(IsWithinRadius(GameManager.Instance.GetPlayerTransform()))
        {
            LoadConversation();

            // Autoplay
            if(conversation.isAutoplayed && !autoPlaying)
            {
                OnInteract();
            }
            // Enter collider to interact
            else
            {
                base.Update();
            }
        }


        if(isFollowing)
        {
            FollowTarget();
        }
    }

    private void LoadConversation()
    {
        // Load the conversation of the NPC based on mission progress
        if (missionsOffered.Count == 0)
        {
            // Random default convo (Temporary until better system is set for default convos)
            conversation = defaultConvos[Random.Range(0, defaultConvos.Count)];
        }
        else
        {
            currentMissionConvos = missionsOffered[0];
            if (startedMission == null)
            {
                conversation = currentMissionConvos.beforeConvo;
            }
            else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Started)
            { 
                conversation = currentMissionConvos.duringConvo;
                Debug.Log("NPC: duringConvo");
            }
            // Objective of mission has been completed but now talking to NPC to close mission
            else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Completed)
            {
                conversation = currentMissionConvos.afterConvo;

                Debug.Log("NPC: afterConvo");
            }
        }
    }

    public override void OnInteract()
    {
        if (!isConversing)
        {
            // NPC has mission to offer
            if(currentMissionConvos != null)
            {
                // Start mission
                if (startedMission == null)
                {
                    // Start mission and store reference because needed to end mission
                    startedMission = MissionManager.Instance.StartMission(currentMissionConvos.missionPrefab);
                    ProgressManager.Instance.AddMission(startedMission);
                    startedMission.OnMissionComplete += HandleOnMissionComplete;
                    startedMission.OnMissionReset += HandleOnMissionReset;
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
                    currentMissionConvos = null;
                }
                else
                {
                    Debug.LogError("Should not be getting to here");
                    return;
                }
            }
            if (!conversation.shouldFollow && !conversation.isAutoplayed)
            {
                NPCFacePlayer();
            }
        }

        if(conversation.isAutoplayed)
        {
            TriggerAutoplay();
        }

        if (conversation.shouldFollow)
        {
            TriggerFollow(player);
        }

        base.OnInteract();
    }


    protected override void OnAutoplayComplete()
    {
        base.OnAutoplayComplete();
        if(conversation.shouldFollow)
        {
            StopFollow();
            ReturnToOrigin();
        }
    }


    void FollowTarget()
    {
        if (targetObject == null)
        {
            Debug.LogError("Object being followed must be assigned");
            return;
        }
        else
        {
            agent.SetDestination(targetObject.transform.position);
        }
    }


    private bool IsWithinRadius(Transform position)
    {
        Debug.Log(Vector3.Distance(originPosition, position.position));
        return Vector3.Distance(originPosition, position.position) < Constants.INTERACT_BOUNDARY_RADIUS;
    }


    public void SetOriginPosition(Vector3 position)
    {
        originPosition = position;
    }


    public void ReturnToOrigin()
    {
        agent.SetDestination(originPosition);
    }


    protected void TriggerFollow(GameObject target)
    {
        targetObject = target;

        if (targetObject == null)
        {
            Debug.LogError("Target to follow must be assigned");
            return;
        }

        isFollowing = true;
    }


    protected void StopFollow()
    {
        isFollowing = false;
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