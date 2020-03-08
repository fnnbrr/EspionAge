using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public enum NPCReactiveAction
{
    None,
    Teleport
}

[System.Serializable]
public class NPCMissionConvos
{
    public MissionsEnum missionsEnum;

    // Type of conversations an NPC can have based on if it is before, during or after a
    public Conversation beforeConvo;
    public Conversation duringConvo;
    public Conversation afterConvo;

    [Header("Reactive Actions")]
    public NPCReactiveAction doOnComplete;
    public Vector3 reactiveActionPosition;
}

public class NPCInteractable : DialogueInteractable
{
    public float boundaryRadius = Constants.INTERACT_BOUNDARY_RADIUS;

    public List<Conversation> defaultConvos;

    public List<NPCMissionConvos> missionsOffered;
    private NPCMissionConvos currentMissionConvos = null;            // Current Mission given by this NPC (should only be 1 per NPC)
    private AMission startedMission;                                 // started mission needed to end mission

    protected NavMeshAgent agent;
    protected GameObject targetObject;

    protected bool isFollowing = false;
    private Vector3 originPosition;
    private Vector3 previousOriginPosition;


    protected override void Start()
    {
        base.Start();
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);

        SetOriginPosition(transform.position);
        previousOriginPosition = transform.position;

        LoadConversation();
    }

    protected override void Update()
    {
        if(IsWithinRadius(originPosition, GameManager.Instance.GetPlayerTransform(), boundaryRadius))
        {
            // Prevent loading during a conversation
            if (!isConversing && !autoPlaying)
            {
                LoadConversation();
            }

            if (conversation.shouldFollow)
            {
                TriggerFollow(player);
            }


            // Autoplay
            if (conversation.autoplayConversation)
            {
                if(!autoPlaying)
                {
                    TriggerAutoplay();
                    OnInteract();
                }
            }
            // Enter collider to interact
            else
            {
                if(conversation.autoInitiate && !isConversing)
				{
                    interactableOn = true;
                    OnInteract();
				}

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
            // If input already disabled, then we won't continue from here and start a conversation
            if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;

            // NPC has mission to offer (Mission should have been loaded in LoadConversation)
            if (currentMissionConvos != null)
            {
                // Start mission
                if (startedMission == null)
                {
                    // Start mission and store reference because needed to end mission
                    startedMission = MissionManager.Instance.StartMission(currentMissionConvos.missionsEnum);
                    startedMission.OnMissionComplete += HandleOnMissionComplete;
                    startedMission.OnMissionComplete += TryReactiveAction;
                    startedMission.OnMissionReset += HandleOnMissionReset;
                    startedMission.OnMissionReset += TryResetReactiveAction;
                }
                else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Started)
                {
                    //Must have this case because it is the catch for a during mission interaction (without this it will go to else)
                }
                // Objective of mission has been completed but now talking to NPC to close mission
                else if (startedMission != null && ProgressManager.Instance.GetMissionStatus(startedMission) == MissionStatusCode.Completed)
                {
                    // Ends current mission with NPC
                    MissionManager.Instance.EndMission(currentMissionConvos.missionsEnum);

                    // Unsubscribe
                    startedMission.OnMissionComplete -= HandleOnMissionComplete;
                    startedMission.OnMissionComplete -= TryReactiveAction;
                    startedMission.OnMissionReset -= HandleOnMissionReset;
                    startedMission.OnMissionReset -= TryResetReactiveAction;
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
            if (!conversation.shouldFollow && !conversation.autoplayConversation)
            {
                NPCFacePlayer();
            }
        }

        base.OnInteract();
    }

    private void TryReactiveAction()
    {
        switch (currentMissionConvos.doOnComplete)
        {
            case NPCReactiveAction.None:
                break;
            case NPCReactiveAction.Teleport:
                SetOriginPosition(currentMissionConvos.reactiveActionPosition);
                transform.position = originPosition;
                break;
            default:
                break;
        }
    }

    private void TryResetReactiveAction()
    {
        switch (currentMissionConvos.doOnComplete)
        {
            case NPCReactiveAction.None:
                break;
            case NPCReactiveAction.Teleport:
                ResetOriginPosition();
                transform.position = originPosition;
                break;
            default:
                break;
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

    public void SetOriginPosition(Vector3 position)
    {
        previousOriginPosition = originPosition;
        originPosition = position;
    }

    public void ResetOriginPosition()
    {
        originPosition = previousOriginPosition;
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
        MissionManager.Instance.RestartMission(currentMissionConvos.missionsEnum);
    }

    private void HandleOnMissionComplete()
    {
        MissionManager.Instance.CompleteMissionObjective(currentMissionConvos.missionsEnum);
    }

    public void NPCFacePlayer()
    {
        Vector3 dirToFace = player.transform.position - transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        StartCoroutine(RotateAnimation(gameObject, rotation, GameManager.Instance.GetMovementController().turnSpeed));
    }


    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.magenta;
        if (!Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.position, boundaryRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(originPosition, boundaryRadius);
        }
    }
}