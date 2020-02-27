using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Chaser : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent agent;
    
    [Header("Chasing")]
    public bool chase = true;
    public Transform targetTransform;

    [Header("Responding")]
    public Vector3 responsePoint;
    
    [Header("Searching")]
    public SphereCollider searchBounds;
    [Tooltip("After hearing a noise, how close should I search to that noise?")]
    public float noiseSearchRadius = 15.0f;

    private const int SAMPLE_ATTEMPTS = 3; // gives NavMesh.SamplePosition room for error

    private Vector3 searchBoundsCenter;
    private float searchBoundsRadius;
    
    [Header("Waiting")]
    public float waitDurationSec = 1.0f;

    private float waitTimer;

    protected enum ActionStates
    {
        Chasing,
        Responding,
        Searching,
        Patrolling,
    }
    
    [Header("For Debugging")]
    [SerializeField] protected ActionStates currentState = ActionStates.Responding;
    [SerializeField] protected ActionStates defaultState = ActionStates.Searching;

    public delegate void CollideWithPlayerAction();
    public event CollideWithPlayerAction OnCollideWithPlayer;

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
        responsePoint = newResponsePoint;
        searchBoundsCenter = wanderPoint;
        searchBoundsRadius = wanderRadius;
    }

    public void HandleTargetsInRange(int numTargetsInRange)
    {
        if (chase && numTargetsInRange > 0 && currentState != ActionStates.Chasing)
        {
            ChaseTarget();
            currentState = ActionStates.Chasing;
        }
        else if (numTargetsInRange == 0 && currentState == ActionStates.Chasing)
        {
            currentState = defaultState;
        }
    }
    
    protected bool WaitComplete()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer > 0)
        {
            agent.SetDestination(transform.position);
            return false;
        }

        waitTimer = waitDurationSec;
        return true;
    }
    
    protected void GotoNextSearchPoint()
    {
        Vector3 randomPoint = searchBoundsCenter + Random.insideUnitSphere * searchBoundsRadius;
        NavMeshHit navHit;

        int attemptsRemaining = SAMPLE_ATTEMPTS;
        while (!NavMesh.SamplePosition(randomPoint, out navHit, searchBoundsRadius, NavMesh.AllAreas))
        {
            attemptsRemaining -= 1;
            if (attemptsRemaining < 0)
            {
                agent.SetDestination(searchBoundsCenter);
            }
        }
 
        agent.SetDestination(navHit.position);
    }

    protected void ChaseTarget()
    {
        Vector3 targetPosition = targetTransform.position;
        Vector3 thisPosition = transform.position;
        Vector3 dirToTarget = thisPosition - targetPosition;
        Vector3 newPos = thisPosition - dirToTarget;

        agent.SetDestination(newPos);
    }

    private void Update()
    {
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
                    currentState = ActionStates.Searching;
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

        // TODO: draw a "?" above the AI
        currentState = ActionStates.Responding;
        
        searchBoundsCenter = other.gameObject.transform.position;
        searchBoundsRadius = noiseSearchRadius;
        agent.SetDestination(searchBoundsCenter);
    }
}
