using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NursePatrol : MonoBehaviour
{
    enum NurseStates
    {
        Patrolling,
        Chasing
    }

    public Transform[] points;
    public bool chase = true;
<<<<<<< HEAD
    [SerializeField] NurseStates currentState;
    public Transform targetTransform;
=======
>>>>>>> 3d86b1b0be74da4a7a6ec9ae029b85e5387fdce0

    private int destPoint = 0;
    private NavMeshAgent agent;
    private bool moving_forward = true;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = false;
        // Current state will be set to patrolling
        currentState = NurseStates.Patrolling;

        if (currentState == NurseStates.Patrolling)
        {
            GotoNextPoint();
        }
    }

    // Cycles through points start->end, then end->start
    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Cycle to next point (patrolling forwards)
        if (moving_forward)
        {
            destPoint += 1;
            if (destPoint == points.Length)
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

    // Follows Target if they are in the field of vision of the nurse, called in FieldOfVision.cs
    public void ChaseTarget()
    {
        Debug.Log("NURSE Chase Target!");
        Debug.Log(transform.position);
        Vector3 targetPosition = targetTransform.position;
        Vector3 dirToTarget = transform.position - targetPosition;
        Vector3 newPos = transform.position - dirToTarget;

        // agent.SetDestination(newPos);
        agent.destination = newPos;
    }

    // Update is called once per frame
    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextPoint();
        }
    }
}
