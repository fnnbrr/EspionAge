using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Responder : Chaser
{
    [Header("Responder")]
    public Transform responsePoint;
    public SphereCollider wanderBounds;
    public float waypointPauseSec = 1.0f;
    private float waypointPauseTimer;

    private Vector3 responsePointPosition;
    private Vector3 wanderBoundsPosition;
    private float wanderBoundsRadius;
    private Vector3 wanderBoundsCenter = Vector3.zero;

    public enum ActionStates
    {
        Responding,
        Wandering,
        Chasing
    }
    [Header("For Debugging")]
    [SerializeField] public ActionStates currentState = ActionStates.Responding;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        waypointPauseTimer = waypointPauseSec;

        // If transform is set, then we can use it's position, otherwise we use responsePointPosition (used in missions)
        agent.SetDestination(responsePoint ? responsePoint.position : responsePointPosition);

        if (wanderBounds)
        {
            wanderBoundsPosition = wanderBounds.transform.position;
            wanderBoundsRadius = wanderBounds.radius;
            wanderBoundsCenter = wanderBounds.center;
        }
    }

    public override void HandleTargetsInRange(int numTargetsInRange)
    {
        if (chase && numTargetsInRange > 0 && currentState != ActionStates.Chasing)
        {
            ChaseTarget();
            currentState = ActionStates.Chasing;
        }
        else if (numTargetsInRange == 0 && currentState == ActionStates.Chasing)
        {
            GotoNextPoint();
            currentState = ActionStates.Wandering;
        }
    }
    
    bool WaypointPauseComplete()
    {
        waypointPauseTimer -= Time.deltaTime;
        if (waypointPauseTimer > 0)
        {
            agent.SetDestination(transform.position);
            return false;
        }

        waypointPauseTimer = waypointPauseSec;
        return true;
    }

    public void InitailizeResponderParameters(Vector3 responsePoint, Vector3 wanderPoint, float wanderRadius)
    {
        responsePointPosition = responsePoint;
        wanderBoundsPosition = wanderPoint;
        wanderBoundsRadius = wanderRadius;
    }
    
    void GotoNextPoint()
    {
        Vector3 randomMovement = wanderBoundsPosition + Random.insideUnitSphere * wanderBoundsRadius + wanderBoundsCenter;

        if (!NavMesh.SamplePosition(randomMovement, out NavMeshHit navHit, wanderBoundsRadius, -1))
        {
            throw new UnityException("NavMesh.SamplePosition failed");
        }
 
        agent.SetDestination(navHit.position);
    }

    void Update()
    {
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            switch (currentState)
            {
                case ActionStates.Wandering:
                    // Choose the next destination point when the agent gets close to the current one.
                    if (WaypointPauseComplete())
                    {
                        GotoNextPoint();
                    }
                    break;
                case ActionStates.Chasing:
                    ChaseTarget();
                    break;
                case ActionStates.Responding:
                    currentState = ActionStates.Wandering;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
