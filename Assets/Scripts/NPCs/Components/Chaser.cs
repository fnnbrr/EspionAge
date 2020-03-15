using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Chaser : MonoBehaviour
    {
        public Transform targetTransform;

        private BaseAi baseAi;
        private NavMeshAgent agent;
        private bool isChasing = false;
        public delegate void CollideWithPlayerAction();
        public event CollideWithPlayerAction OnCollideWithPlayer;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !baseAi.currentState.Equals("Chasing") && agent.enabled)
            {
                ChaseTarget();
                baseAi.SetState("Chasing");
            }
            else if (numTargetsInRange == 0 && baseAi.currentState.Equals("Chasing"))
            {
                baseAi.SetState(baseAi.defaultState);
            }
        }
        
        public void ChaseTarget()
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