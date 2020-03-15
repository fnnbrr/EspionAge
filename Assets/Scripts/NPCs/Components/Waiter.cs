using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Waiter : MonoBehaviour
    {
        public float defaultWaitDuration = 1.0f;

        private float waitTimer = 0f;

        private NavMeshAgent agent;
        private bool isWaiting = false;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }

        protected bool WaitComplete()
        {
            return WaitComplete(defaultWaitDuration);
        }

        protected bool WaitComplete(float waitDuration)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer < waitDuration)
            {
                isWaiting = true;
                agent.enabled = false;
                return false;
            }
            
            isWaiting = false;
            agent.enabled = true;
            waitTimer = 0f;
            return true;
        }
    }
}