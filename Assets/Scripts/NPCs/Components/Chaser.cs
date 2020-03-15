using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Chaser : MonoBehaviour
    {
        public Transform targetTransform;

        private NavMeshAgent agent;
        private bool isChasing = false;
        public delegate void CollideWithPlayerAction();
        public event CollideWithPlayerAction OnCollideWithPlayer;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !isChasing && agent.enabled)
            {
                isChasing = true;
                ChaseTarget();
            }
            else if (numTargetsInRange == 0 && isChasing)
            {
                isChasing = false;
            }
        }
        
        protected void ChaseTarget()
        {
            Vector3 targetPosition = targetTransform.position;
            Vector3 thisPosition = transform.position;
            Vector3 dirToTarget = thisPosition - targetPosition;
            Vector3 newPos = thisPosition - dirToTarget;

            agent.SetDestination(newPos);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
            {
                OnCollideWithPlayer?.Invoke();
            }
        }
    }
}