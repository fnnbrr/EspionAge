using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Waiter : MonoBehaviour
    {
        private float waitTimer = 0f;

        private BaseAi baseAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        public bool WaitComplete(float waitDuration=1.0f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer < waitDuration)
            {
                agent.enabled = false;
                return false;
            }
            
            agent.enabled = true;
            waitTimer = 0f;
            return true;
        }
    }
}