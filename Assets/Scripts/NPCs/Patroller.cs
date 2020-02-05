using System;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : Chaser
{
    [Header("Patroller")]
    public Transform patrolWaypoints;
    public float waypointPauseSec = 1.0f;
    private float waypointPauseTimer;

    public enum ActionStates
    {
        Patrolling,
        Chasing
    }
    [Header("For Debugging")]
    [SerializeField] public ActionStates currentState = ActionStates.Patrolling;

    private List<Vector3> points = new List<Vector3>();
    private int destinationCount;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        waypointPauseTimer = waypointPauseSec;

        if (patrolWaypoints)
        {
            foreach (Transform childWaypoint in patrolWaypoints)
            {
                points.Add(childWaypoint.position);
            }
        }
    }

    public void SetPoints(List<Vector3> newPoints)
    {
        points.Clear();
        points = new List<Vector3>(newPoints);
    }

    public override void HandleTargetsInRange(int numTargetsInRange)
    {
        if (chase && numTargetsInRange > 0 && currentState != ActionStates.Chasing)
        {
            ChaseTarget();
            currentState = ActionStates.Chasing;
        }
        else if (numTargetsInRange == 0 && currentState != ActionStates.Patrolling)
        {
            GotoNextPoint();
            currentState = ActionStates.Patrolling;
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

    void Update()
    {
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            switch (currentState)
            {
                case ActionStates.Patrolling:
                    // Choose the next destination point when the agent gets close to the current one.
                    if (WaypointPauseComplete())
                    {
                        GotoNextPoint();
                    }
                    break;
                case ActionStates.Chasing:
                    ChaseTarget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
