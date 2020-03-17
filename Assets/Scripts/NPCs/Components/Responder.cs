using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    public class Responder : MonoBehaviour
    {
        public float movementSpeed = 8.0f;
        public Vector3 responsePoint;


        public void InitializeResponderParameters(Vector3 newResponsePoint)
        {
            responsePoint = newResponsePoint;
            // baseNavAi.SetState("Responding"); TODO replace with Event
            //baseNavAi.agent.SetDestination(responsePoint); TODO replace with Event
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Noise")) return;
            
            responsePoint = other.gameObject.transform.position;
            // baseNavAi.agent.SetDestination(responsePoint); TODO replace with Event
            
            // baseNavAi.SetState("Responding"); TODO replace with Event
        }
    }
}