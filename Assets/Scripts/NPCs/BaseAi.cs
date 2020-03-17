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
            agent.avoidancePriority = Random.Range(0, 99);
        }

        public abstract void SetState(string newState);
    
        protected abstract void Update();

        public abstract void ToggleAnimations(bool toggle);
    }
}
