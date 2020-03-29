using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCs.Components
{
    public class Enemy : MonoBehaviour
    {
        public static readonly HashSet<Enemy> Enemies = new HashSet<Enemy>();
        
        public event Action OnCollideWithPlayer;

        private void OnEnable()
        {
            Enemies.Add(this);
        }

        private void OnDisable()
        {
            Enemies.Remove(this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
            {
                OnCollideWithPlayer?.Invoke();
                Chaser.numChasersActive = 0;
            }
        }
    }
}
