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
        private static int _spawnOrder = 0;

        public virtual void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
            agent.avoidancePriority = _spawnOrder++ % 100;
        }

        public abstract void SetState(string newState);
    
        protected abstract void Update();

        public abstract void ToggleAnimations(bool toggle);
    }
}
