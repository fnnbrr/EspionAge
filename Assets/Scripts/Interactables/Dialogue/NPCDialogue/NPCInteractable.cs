using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;

[System.Serializable]
public enum ENPCReactiveAction
{
    None,
    Teleport,
    Destroy,
    StartMission
}

[System.Serializable]
public class NPCReactiveAction
{
    public ENPCReactiveAction actionType;

    [ShowIf("ShowIfTeleport")]
    [AllowNesting]
    public Vector3 actionPosition;

    // bug opened with NaughtyAttributes since this is not working with Lists currently https://github.com/dbrizov/NaughtyAttributes/issues/142
    [ShowIf("ShowIfDestroy")] 
    [AllowNesting]
    public List<GameObject> objects;

    [ShowIf("ShowIfStartMission")]
    [AllowNesting]
    public MissionsEnum missionToStart;

    public bool ShowIfTeleport => actionType == ENPCReactiveAction.Teleport;
    public bool ShowIfDestroy => actionType == ENPCReactiveAction.Destroy;
    public bool ShowIfStartMission => actionType == ENPCReactiveAction.StartMission;
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
    public List<NPCReactiveAction> doOnComplete;
}

public class NPCInteractable : Interactable
{
    public Conversation conversation;

    [ValidateInput("IsGreaterEqualThanInteractRadius", "boundaryRadius must be greater than or equal to interactRadius")]
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

    private Quaternion originRotation;


    protected override void Start()
    {
        base.Start();
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);

        SetOriginPosition(transform.position);
        previousOriginPosition = transform.position;

        SetOriginRotation(transform.rotation);

        LoadConversation();
    }

    protected override void Update()
    {
        if (IsWithinRadius(originPosition, GameManager.Instance.GetPlayerTransform(), boundaryRadius))
        {
            // Prevent loading during a conversation
            if (!DialogueManager.Instance.IsActiveConversation(conversation))
            {
                LoadConversation();

                if (conversation != null && (conversation.autoplayConversation || // Autoplay
                    (conversation.autoInitiate && !DialogueManager.Instance.CheckIsAdvancing()))) // Prevent from auto-initiating in the middle of advancing a conversation
                {
                    OnInteract();
                }
            }
        }

        // TODO: Boundary Radius should always be >= Interact Radius Possible issues will arise otherwise
        // This is so the animations for InteractBox work
        // Ensure player cannot interact with another character if already advancing a conversation
        if (conversation != null && !DialogueManager.Instance.CheckIsAdvancing() && !conversation.autoplayConversation && !conversation.autoInitiate)
        {
            base.Update();
        }
    }

    private void LoadConversation()
    {
        // Load the conversation of the NPC based on mission progress
        if (missionsOffered.Count == 0)
        {
            // Random default convo (Temporary until better system is set for default convos)
            conversation = defaultConvos.Count > 0 ? defaultConvos[Random.Range(0, defaultConvos.Count)] : null;
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
        if (!DialogueManager.Instance.CheckIsAdvancing())
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
                    startedMission.OnMissionComplete += TryReactiveActions;
                    startedMission.OnMissionReset += HandleOnMissionReset;
                    startedMission.OnMissionReset += TryResetReactiveActions;
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

                    // Cleanup any reactive actions now
                    CleanupReactiveActions();

                    // Unsubscribe
                    startedMission.OnMissionComplete -= HandleOnMissionComplete;
                    startedMission.OnMissionComplete -= TryReactiveActions;
                    startedMission.OnMissionReset -= HandleOnMissionReset;
                    startedMission.OnMissionReset -= TryResetReactiveActions;
                    startedMission = null;
                    missionsOffered.RemoveAt(0);

                    DialogueManager.Instance.OnFinishConversation += HandleEndMissionFinishConversation;
                }
                else
                {
                    Debug.LogError("Should not be getting to here");
                    return;
                }
            }

            if (!conversation.autoplayConversation)
            {
                NPCFacePlayer();
            }
            DialogueManager.Instance.OnFinishConversation += NPCFaceOriginalRotation;
            DialogueManager.Instance.StartConversation(conversation);
            TriggerOnInteractEnd();
        }
    }

    private void TryReactiveActions()
    {
        currentMissionConvos.doOnComplete.ForEach(a =>
        {
            switch (a.actionType)
            {
                case ENPCReactiveAction.None:
                    break;
                case ENPCReactiveAction.Teleport:
                    SetOriginPosition(a.actionPosition);
                    transform.position = originPosition;
                    break;
                case ENPCReactiveAction.Destroy:
                    a.objects.ForEach(o => o.SetActive(false));
                    break;
                case ENPCReactiveAction.StartMission:
                    break;
                default:
                    break;
            }
        });
    }

    private void TryResetReactiveActions()
    {
        currentMissionConvos.doOnComplete.ForEach(a =>
        {
            switch (a.actionType)
            {
                case ENPCReactiveAction.None:
                    break;
                case ENPCReactiveAction.Teleport:
                    ResetOriginPosition();
                    transform.position = originPosition;
                    break;
                case ENPCReactiveAction.Destroy:
                    a.objects.ForEach(o => o.SetActive(true));
                    break;
                case ENPCReactiveAction.StartMission:
                    break;
                default:
                    break;
            }
        });
    }

    private void CleanupReactiveActions()
    {
        currentMissionConvos.doOnComplete.ForEach(a =>
        {
            switch (a.actionType)
            {
                case ENPCReactiveAction.None:
                    break;
                case ENPCReactiveAction.Teleport:
                    break;
                case ENPCReactiveAction.Destroy:
                    a.objects.ForEach(Destroy);
                    break;
                case ENPCReactiveAction.StartMission:
                    break;
                default:
                    break;
            }
        });
    }

    private void ReactiveActionFinishConversation()
    {
        currentMissionConvos.doOnComplete.ForEach(a =>
        {
            switch (a.actionType)
            {
                case ENPCReactiveAction.None:
                    break;
                case ENPCReactiveAction.Teleport:
                    break;
                case ENPCReactiveAction.Destroy:
                    break;
                case ENPCReactiveAction.StartMission:
                    MissionManager.Instance.StartMission(a.missionToStart);
                    break;
                default:
                    break;
            }
        });
    }

    private void HandleEndMissionFinishConversation(Conversation finishedConversation)
    {
        if (finishedConversation != conversation) return;
        DialogueManager.Instance.OnFinishConversation -= HandleEndMissionFinishConversation;

        ReactiveActionFinishConversation();

        currentMissionConvos = null;
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

    public void SetOriginRotation(Quaternion rotation)
	{
        originRotation = rotation;
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

    public void NPCFaceOriginalRotation(Conversation finishedConvo)
	{
        if (finishedConvo != conversation) return;

        DialogueManager.Instance.OnFinishConversation -= NPCFaceOriginalRotation;
        StopAllCoroutines();
        StartCoroutine(RotateToOriginal(1.0f));
	}

    private IEnumerator RotateToOriginal(float waitTime)
	{
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(RotateAnimation(gameObject, originRotation, GameManager.Instance.GetMovementController().turnSpeed));
    }

    private bool IsGreaterEqualThanInteractRadius(float value)
    {
        return value >= interactRadius;
    }

    public void ResetAllConversations()
    {
        defaultConvos.Clear();
        conversation = null;
        missionsOffered.Clear();
        currentMissionConvos = null;
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