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

        public static int numChasersActive = 0;
        
        public event Action OnSeePlayer;
        public event Action OnLosePlayer;
        public event Action OnReacquireTarget;

        [FMODUnity.ParamRef] private static string playerChased;

        private FieldOfVision fieldOfVision;

        public static void ResetChaserCount()
        {
            numChasersActive = 0;  // Allows Birdie to spawn with full stealth/no systems aware of her presence
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 0f);
        }

        private void Start()
        {
            InvokeRepeating(nameof(ReacquireTarget), 0f, reacquireInterval);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 0f);

            fieldOfVision = GetComponent<FieldOfVision>();
            if (fieldOfVision)
            {
                fieldOfVision.OnTargetsUpdated += HandleTargetsInRange;
            }
        }

        public void HandleTargetsInRange(int numTargetsInRange)
        {
            if (numTargetsInRange > 0 && !isChasing)
            {
                isChasing = true;
                numChasersActive += 1;
                
                if (numChasersActive == 1)
                {
                    // This means that the current chaser is the 1st to begin chasing Birdie
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 1f);
                }
                
                OnSeePlayer?.Invoke();
            }

            else if (numTargetsInRange == 0 && isChasing)
            {
                isChasing = false;

                if (numChasersActive > 0)
                {
                    numChasersActive -= 1;
                    if (numChasersActive == 0)
                    {
                        // This means that the current chaser is the last to stop chasing Birdie
                        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(playerChased, 0f);
                    }
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
    }
}