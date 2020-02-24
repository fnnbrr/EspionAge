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
    private NPCMissionConvos currentMissionConvos = null;            // Current Mission given by this NPC (should only be 1 per NPC)
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
        if(IsWithinBoundaryRadius(GameManager.Instance.GetPlayerTransform()))
        {
            // Prevent loading during a conversation
            if (!isConversing && !autoPlaying)
            {
                LoadConversation();
            }

            // Autoplay
            if(conversation.isAutoplayed)
            {
                if(!autoPlaying)
                {
                    OnInteract();
                }
            }
            // Enter collider to interact
            else
            {
                base.Update();
            }
        }
        else
        {
            if (isFollowing)
            {
                StopFollow();
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
            }
            // Objective of mission has been completed but now talking to NPC to close mission
            else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Completed)
            {
                conversation = currentMissionConvos.afterConvo;
            }
        }
    }

    public override void OnInteract()
    {
        if (!isConversing)
        {
            // NPC has mission to offer (Mission should have been loaded in LoadConversation)
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
                else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Started)
                {
                    //Must have this case because it is the catch for a during mission interaction (without this it will go to else)
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

    protected override void OnTriggerEnter(Collider other)
    {
        if (!autoPlaying)
        {
            base.OnTriggerEnter(other);
        }
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

    // Not sure if we'll need this funtionality but putting it here anyway
    // Would happen when shouldFollow is true and autoPlaying is false for that conversation
    protected override void EndConversation()
    {
        base.EndConversation();

        if(isFollowing && !autoPlaying)
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

    private bool IsWithinBoundaryRadius(Transform targetTransform)
    {
        return Vector3.Distance(originPosition, targetTransform.position) < Constants.INTERACT_BOUNDARY_RADIUS;
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

    public void NPCFacePlayer()
    {
        Vector3 dirToFace = player.transform.position - transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        StartCoroutine(RotateAnimation(gameObject, rotation, player.GetComponent<PlayerController>().turnSpeed));
    }
}