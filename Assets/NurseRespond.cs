using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class NurseRespond : MonoBehaviour
{
    public Transform responsePoint;
    public SphereCollider wanderBounds;

    [Header("Chase")]
    public bool chase = true;
    public Transform targetTransform;
    public float waypointPauseSec = 1.0f;
    private float waypointPauseTimer;

    public enum NurseStates
    {
        Responding,
        Wandering,
        Chasing
    }
    [Header("For Debugging")]
    [SerializeField] public NurseStates currentState = NurseStates.Responding;

    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        waypointPauseTimer = waypointPauseSec;

        agent.SetDestination(responsePoint.position);

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;
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
    
    void GotoNextPoint()
    {
        float wanderRadius = wanderBounds.radius;
        Vector3 randomMovement = Random.insideUnitSphere * wanderRadius + wanderBounds.center;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomMovement, out navHit, wanderRadius, -1);
 
        agent.SetDestination(navHit.position);
    }

    public void ChaseTarget()
    {
        Vector3 targetPosition = targetTransform.position;
        Vector3 dirToTarget = transform.position - targetPosition;
        Vector3 newPos = transform.position - dirToTarget;

        agent.SetDestination(newPos);
    }

    void Update()
    {
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            switch (currentState)
            {
                case NurseStates.Wandering:
                    // Choose the next destination point when the agent gets close to the current one.
                    if (WaypointPauseComplete())
                    {
                        GotoNextPoint();
                    }
                    break;
                case NurseStates.Chasing:
                    ChaseTarget();
                    break;
                case NurseStates.Responding:
                    currentState = NurseStates.Wandering;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
