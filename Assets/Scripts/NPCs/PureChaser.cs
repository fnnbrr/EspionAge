using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PureChaser : MonoBehaviour
{
    public Transform targetTransform;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
    }

    protected void ChaseTarget()
    {
        if (targetTransform)
        {
            agent.SetDestination(targetTransform.position);
        }
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        ChaseTarget();
    }
}
