using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    public class Chaser : MonoBehaviour
    {
        public float movementSpeed = 10.0f;
        public Transform targetTransform;
        private bool isChasing = false;

        public delegate void CollideWithPlayerAction();
        public event CollideWithPlayerAction OnCollideWithPlayer;

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !isChasing)
            {
                isChasing = true;
                //agent.SetDestination(targetTransform.position); TODO replace with Event
                // baseNavAi.SetState("Chasing"); TODO replace with Event
            }

            else if (numTargetsInRange == 0 && isChasing)
            {
                isChasing = false;
                // TODO replace with Event
            }
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