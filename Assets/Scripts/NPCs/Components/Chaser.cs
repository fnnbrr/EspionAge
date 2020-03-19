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

        private static int _numChasersActive = 0;
        
        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnReacquireTarget;
        public event Action OnCollideWithPlayer;

        [FMODUnity.ParamRef]
        public string playerChased;

        private void Start()
        {
            InvokeRepeating(nameof(ReacquireTarget), 0f, reacquireInterval);
            Debug.Log("0f");
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 0f);
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !isChasing)
            {
                isChasing = true;
                _numChasersActive += 1;
                if (_numChasersActive == 1)
                {
                    // This means that the current chaser is the 1st to begin chasing Birdie
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 1f);
                }
                
                OnSeePlayer?.Invoke();
            }

            else if (numTargetsInRange == 0 && isChasing)
            {
                isChasing = false;
                _numChasersActive -= 1;
                if (_numChasersActive == 0)
                {
                    // This means that the current chaser is the last to stop chasing Birdie
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 0f);
                }
                
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
                _numChasersActive = 0;
            }
        }
    }
}