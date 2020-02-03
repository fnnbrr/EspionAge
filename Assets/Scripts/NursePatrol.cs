using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NursePatrol : MonoBehaviour
{
    public Transform patrolWaypoints;

    [Header("Chase")]
    public bool chase = true;
    public Transform targetTransform;
    public float waypointPauseSec = 1.0f;
    private float waypointPauseTimer;

    public enum NurseStates
    {
        Patrolling,
        Chasing
    }
    [Header("For Debugging")]
    [SerializeField] public NurseStates currentState = NurseStates.Patrolling;

    private NavMeshAgent agent;
    private List<Vector3> points = new List<Vector3>();
    private int destinationCount;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        waypointPauseTimer = waypointPauseSec;

        if (patrolWaypoints)
        {
            foreach (Transform childWaypoint in patrolWaypoints)
            {
                points.Add(childWaypoint.position);
            }
        }

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;
    }

    public void SetPoints(List<Vector3> newPoints)
    {
        points.Clear();
        points = new List<Vector3>(newPoints);
    }

    public void HandleTargetsInRange(int numTargetsInRange)
    {
        if (chase && numTargetsInRange > 0 && currentState != NurseStates.Chasing)
        {
            ChaseTarget();
            currentState = NurseStates.Chasing;
        }
        else if (numTargetsInRange == 0 && currentState != NurseStates.Patrolling)
        {
            GotoNextPoint();
            currentState = NurseStates.Patrolling;
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
    
    // Cycles through points start->end, then end->start
    void GotoNextPoint()
    {
        // Returns if only the starting position is present
        if (points.Count < 2)
        {
            return;
        }

        // Set the agent to go to the currently selected destination.
        agent.SetDestination(points[Utils.PingPong(destinationCount, points.Count - 1)]);
        destinationCount++;
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
                case NurseStates.Patrolling:
                    // Choose the next destination point when the agent gets close to the current one.
                    if (WaypointPauseComplete())
                    {
                        GotoNextPoint();
                    }
                    break;
                case NurseStates.Chasing:
                    ChaseTarget();
                    break;
                default:
                    break;
            }
        }
    }
}
