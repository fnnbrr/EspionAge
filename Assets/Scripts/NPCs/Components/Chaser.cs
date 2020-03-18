using System;
using UnityEngine;

namespace NPCs.Components
{
    public class Chaser : MonoBehaviour
    {
        public float movementSpeed = 10.0f;
        [Tooltip("While chasing the player, how often should I update my target position?")]
        public float reacquireInterval = 0.1f;
        public bool isChasing = false;

        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnReacquireTarget;
        public event Action OnCollideWithPlayer;

        private void Start()
        {
            InvokeRepeating(nameof(ReacquireTarget), 0f, reacquireInterval);
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !isChasing)
            {
                isChasing = true;
                OnSeePlayer?.Invoke();
            }

            else if (numTargetsInRange == 0 && isChasing)
            {
                isChasing = false;
                OnLosePlayer?.Invoke();
            }
        }
        
        private void ReacquireTarget()
        {
            if (isChasing)
            {
                OnReacquireTarget?.Invoke();
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