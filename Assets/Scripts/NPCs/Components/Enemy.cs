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

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag(Constants.TAG_PLAYER)) return;
            
            Chaser.ResetChaserCount();
            OnCollideWithPlayer?.Invoke();
        }

        public static float DistToClosestEnemy(Vector3 queryPosition)
        {
            float minDistance = Mathf.Infinity;
            
            foreach (Enemy curEnemy in Enemies)
            {
                float curDistance = (curEnemy.transform.position - queryPosition).sqrMagnitude;
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                }
            }
            return Mathf.Sqrt(minDistance);
        }
    }
}
