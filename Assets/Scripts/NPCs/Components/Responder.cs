using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Responder : MonoBehaviour
    {
        public Vector3 responsePoint;
        public bool initiallyResponding = false;

        private BaseAi baseAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        private void Start()
        {
            if (initiallyResponding)
            {
                baseAi.SetState("Responding");
                agent.SetDestination(responsePoint);
            }
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
            
            baseAi.SetState("Responding");
        
            responsePoint = other.gameObject.transform.position;
            agent.SetDestination(responsePoint);
            
            Searcher relatedSearcher = GetComponentInParent<Searcher>();
            if (relatedSearcher != null)
            {
                relatedSearcher.searchBoundsCenter = responsePoint;
            }
        }
    }
}