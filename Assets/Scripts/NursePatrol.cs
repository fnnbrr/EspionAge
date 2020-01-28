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

    enum NurseStates
    {
        Patrolling,
        Chasing
    }
    [Header("For Debugging")]
    [SerializeField] NurseStates currentState = NurseStates.Patrolling;

    private NavMeshAgent agent;
    private List<Vector3> points = new List<Vector3>();
    private int destinationCount;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

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

    // Cycles through points start->end, then end->start
    void GotoNextPoint()
    {
        currentState = NurseStates.Patrolling;

        // Returns if only the starting position is present
        if (points.Count < 2)
        {
            return;
        }

        // Set the agent to go to the currently selected destination.
        agent.SetDestination(points[Utils.PingPong(destinationCount, points.Count - 1)]);
        destinationCount++;
    }

    // Follows Target if they are in the field of vision of the nurse, called in FieldOfVision.cs
    public void ChaseTarget()
    {
        currentState = NurseStates.Chasing;

        Vector3 targetPosition = targetTransform.position;
        Vector3 dirToTarget = transform.position - targetPosition;
        Vector3 newPos = transform.position - dirToTarget;

        agent.SetDestination(newPos);
    }

    // Update is called once per frame
    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextPoint();
        }
    }
}
