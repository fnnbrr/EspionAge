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
    private Vector3 wanderBoundsCenter;
    private float wanderBoundsRadius;

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
            wanderBoundsCenter = wanderBounds.center;
            wanderBoundsRadius = wanderBounds.radius;
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
        wanderBoundsCenter = wanderPoint;
        wanderBoundsRadius = wanderRadius;
    }
    
    void GotoNextPoint()
    {
        Vector3 randomPoint = wanderBoundsCenter + Random.insideUnitSphere * wanderBoundsRadius;
        
        if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit navHit, wanderBoundsRadius, NavMesh.AllAreas))
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
                    if (WaypointPauseComplete())
                    {
                        currentState = ActionStates.Wandering;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Noise")) return;

        // TODO: draw a "?" above the Responder
        currentState = ActionStates.Responding;
        
        wanderBoundsCenter = other.gameObject.transform.position;
        wanderBoundsRadius = 15.0f;
        agent.SetDestination(wanderBoundsCenter);
    }
}
