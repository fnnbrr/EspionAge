using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PureChaser : MonoBehaviour
{
    public Transform targetTransform;
    public float startChaseRadius = 100f;

    private NavMeshAgent agent;
    private bool shouldChase = false;

    public event Chaser.CollideWithPlayerAction OnCollideWithPlayer;

    private void Awake()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    private void ChaseTarget()
    {
        if (shouldChase && targetTransform)
        {
            agent.SetDestination(targetTransform.position);
        }
    }

    private void Update()
    {
        if (!agent.isOnNavMesh) return;

        if (!shouldChase && Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position) <= startChaseRadius)
        {
            shouldChase = true;
        }

        ChaseTarget();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            OnCollideWithPlayer?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, startChaseRadius);
    }
}
