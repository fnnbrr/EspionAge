using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseNavAi : MonoBehaviour
    {
        [HideInInspector] public NavMeshAgent agent;
        private static int _spawnOrder = 0;
        
        protected virtual void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
            agent.avoidancePriority = _spawnOrder++ % 100;
        }
    }

    
    public abstract class BaseStateAi<T> : BaseNavAi where T : Enum
    {
        [Header("State Management")]
        [SerializeField] public T prevState;
        [SerializeField] public T currentState;
        [SerializeField] public T nextState;

        protected abstract void SetState(T newState);
    }
}
