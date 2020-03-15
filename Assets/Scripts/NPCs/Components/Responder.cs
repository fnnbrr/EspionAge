using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Responder : MonoBehaviour
    {
        public Vector3 responsePoint;

        private NavMeshAgent agent;
        private bool isResponding = false;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }

        private void Start()
        {
            agent.SetDestination(responsePoint);
        }

        public void InitializeResponderParameters(Vector3 newResponsePoint)
        {
            isResponding = true;
            responsePoint = newResponsePoint;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Noise") && agent.enabled) return;
        
            isResponding = true;
        
            responsePoint = other.gameObject.transform.position;
            agent.SetDestination(responsePoint);
        }
    }
}