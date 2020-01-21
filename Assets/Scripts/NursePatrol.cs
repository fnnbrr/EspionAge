using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NursePatrol : MonoBehaviour
{
    public Transform Patrol_Waypoints;

    private List<Transform> points = new List<Transform>();
    private int destPoint = 0;
    private NavMeshAgent agent;
    private bool moving_forward = true;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        foreach (Transform Waypoint in Patrol_Waypoints)
        {
            points.Add(Waypoint);
        }

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;

        GotoNextPoint();
    }

    // Cycles through points start->end, then end->start
    void GotoNextPoint()
    {
        // Returns if only the starting position is present
        if (points.Count < 2)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Cycle to next point (patrolling forwards)
        if (moving_forward)
        {
            destPoint += 1;
            if (destPoint == points.Count)
            {
                moving_forward = false;
                destPoint -= 2;
            }
        }

        // Cycle to next point (patrolling backwards)
        else
        {
            destPoint -= 1;
            if (destPoint == -1)
            {
                moving_forward = true;
                destPoint += 2;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPoint();
    }
}
