using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Chaser : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    protected Animator animator;

    [Header("Chasing")]
    public bool chase = true;
    public Transform targetTransform;

    [Header("Responding")]
    public Vector3 responsePoint;
    
    [Header("Searching")]
    public SphereCollider searchBounds;
    [Tooltip("After hearing a noise, how close should I search to that noise?")]
    public float noiseSearchRadius = 15.0f;

    private Vector3 searchBoundsCenter;
    private float searchBoundsRadius;
    
    [Header("Waiting")]
    public float waitDurationSec = 1.0f;
    protected bool isWaiting = false;

    protected float waitTimer;

    protected enum ActionStates
    {
        Chasing,
        Responding,
        Searching,
        Patrolling,
    }
    
    [Header("Question Mark Icon")]
    public GameObject questionMark;
    
    [Header("For Debugging")]
    [SerializeField] protected ActionStates currentState = ActionStates.Responding;
    [SerializeField] protected ActionStates defaultState = ActionStates.Searching;

    public delegate void CollideWithPlayerAction();
    public event CollideWithPlayerAction OnCollideWithPlayer;

    private void Awake()
    {
        animator = Utils.GetRequiredComponentInChildren<Animator>(this);
    }

    public void Start()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;
        
        waitTimer = waitDurationSec;

        // If transform is set, then we can use it's position, otherwise we use responsePointPosition (used in missions)
        agent.SetDestination(responsePoint);

        if (!searchBounds) return;
        
        searchBoundsCenter = searchBounds.center;
        searchBoundsRadius = searchBounds.radius;
    }
    
    public void InitializeResponderParameters(Vector3 newResponsePoint, Vector3 wanderPoint, float wanderRadius)
    {
        SetState(ActionStates.Responding);
        responsePoint = newResponsePoint;
        searchBoundsCenter = wanderPoint;
        searchBoundsRadius = wanderRadius;
    }

    public void HandleTargetsInRange(int numTargetsInRange)
    {
        if (chase && numTargetsInRange > 0 && currentState != ActionStates.Chasing)
        {
            ChaseTarget();
            SetState(ActionStates.Chasing);
        }
        else if (numTargetsInRange == 0 && currentState == ActionStates.Chasing)
        {
            SetState(defaultState);
        }
    }
    
    protected bool WaitComplete()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0)
        {
            isWaiting = true;
            agent.enabled = false;
            return false;
        }

        waitTimer = waitDurationSec;
        agent.enabled = true;
        isWaiting = false;
        return true;
    }
    
    protected void GotoNextSearchPoint()
    {
        Vector3 randomPoint = searchBoundsCenter + Random.insideUnitSphere * searchBoundsRadius;
        NavMeshHit navHit;

        if (!NavMesh.SamplePosition(randomPoint, out navHit, searchBoundsRadius, NavMesh.AllAreas))
        {
            navHit.position = searchBoundsCenter;
        }

        animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);

        agent.SetDestination(navHit.position);
    }

    protected void ChaseTarget()
    {
        animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);

        Vector3 targetPosition = targetTransform.position;
        Vector3 thisPosition = transform.position;
        Vector3 dirToTarget = thisPosition - targetPosition;
        Vector3 newPos = thisPosition - dirToTarget;

        agent.SetDestination(newPos);
    }

    private void Update()
    {
        // TEMP, we gotta refactor this logic to make this work better and be more robust
        if (isWaiting)
        {
            animator.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                agent.enabled = true;
                isWaiting = false;
            }
            return;
        }

        if (!agent.isOnNavMesh || agent.pathPending || !(agent.remainingDistance < 0.5f)) return;
        
        // Choose the next destination point when the agent gets close to the current one.
        switch (currentState)
        {
            case ActionStates.Searching:
                if (WaitComplete())
                {
                    GotoNextSearchPoint();
                }
                break;
            case ActionStates.Chasing:
                ChaseTarget();
                break;
            case ActionStates.Responding:
                if (WaitComplete())
                {
                    SetState(ActionStates.Searching);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            OnCollideWithPlayer?.Invoke();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Noise")) return;
        
        SetState(ActionStates.Responding);
        
        searchBoundsCenter = other.gameObject.transform.position;
        searchBoundsRadius = noiseSearchRadius;
        agent.SetDestination(searchBoundsCenter);

        waitTimer = waitDurationSec;
    }

    protected void SetState(ActionStates newState)
    {
        // Ensures that question mark is properly shown/hidden
        switch (newState)
        {
            case ActionStates.Searching:
                ShowQuestionMark();
                break;
            case ActionStates.Responding:
                ShowQuestionMark();
                break;
            case ActionStates.Chasing:
                HideQuestionMark();
                break;
            case ActionStates.Patrolling:
                HideQuestionMark();
                break;
            default:
                HideQuestionMark();
                break;
        }
        
        currentState = newState;
    }
    
    protected void ShowQuestionMark()
    {
        questionMark.SetActive(true);
    }

    protected void HideQuestionMark()
    {
        questionMark.SetActive(false);
    }
}
