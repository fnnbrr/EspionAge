using System;
using UnityEngine;

namespace NPCs.Components
{
    public class Chaser : MonoBehaviour
    {
        public float movementSpeed = 10.0f;
        private bool isChasing = false;

        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnCollideWithPlayer;

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

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
            {
                OnCollideWithPlayer?.Invoke();
            }
        }
    }
}