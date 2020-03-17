using System;
using UnityEngine;

namespace NPCs.Components
{
    public class Responder : MonoBehaviour
    {
        public float movementSpeed = 8.0f;
        public Vector3 responsePoint;

        public event Action OnStartResponding;

        public void InitializeResponderParameters(Vector3 newResponsePoint)
        {
            responsePoint = newResponsePoint;
            OnStartResponding?.Invoke();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Noise")) return;
            
            responsePoint = other.gameObject.transform.position;
            OnStartResponding?.Invoke();
        }
    }
}