using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Chaser : MonoBehaviour
    {
        public float movementSpeed = 10.0f;
        public Transform targetTransform;

        private BaseAi baseAi;
        private NavMeshAgent agent;
        
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
        }
        
        public void ChaseTarget()
        {
            agent.SetDestination(targetTransform.position);
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