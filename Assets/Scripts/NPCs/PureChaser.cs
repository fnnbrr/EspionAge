using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PureChaser : MonoBehaviour
{
    public Transform targetTransform;

    private NavMeshAgent agent;

    public event Chaser.CollideWithPlayerAction OnCollideWithPlayer;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            OnCollideWithPlayer?.Invoke();
        }
    }
}
