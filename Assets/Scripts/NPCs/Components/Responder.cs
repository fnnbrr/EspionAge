using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Responder : MonoBehaviour
    {
        public float movementSpeed = 8.0f;
        public Vector3 responsePoint;

        private BaseAi baseAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        public void InitializeResponderParameters(Vector3 newResponsePoint)
        {
            responsePoint = newResponsePoint;
            baseAi.SetState("Responding");
            agent.SetDestination(responsePoint);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Noise") && agent.enabled) return;
            
            responsePoint = other.gameObject.transform.position;
            agent.SetDestination(responsePoint);
            
            baseAi.SetState("Responding");
        }
    }
}