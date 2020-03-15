using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseAi : MonoBehaviour
    {
        [Header("State Management")]
        [SerializeField] public string currentState;
        [SerializeField] public string defaultState;
        
        [HideInInspector] public NavMeshAgent agent;

        public void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }

        public void Start()
        {
            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            agent.autoBraking = false;
        }

        public abstract void SetState(string newState);
    
        protected abstract void Update();
    }
}
