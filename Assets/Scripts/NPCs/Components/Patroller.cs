using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Patroller : MonoBehaviour
    {
        public Transform patrolWaypoints;
        
        private NavMeshAgent agent;
        private List<Vector3> patrolPositions = new List<Vector3>();
        private int destinationCount;

        private void Awake()
        {
            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        }
        
        private void Start()
        {
            if (patrolWaypoints)
            {
                foreach (Transform childWaypoint in patrolWaypoints)
                {
                    patrolPositions.Add(childWaypoint.position);
                }
            }
        }
        
        public void SetPoints(List<Vector3> newPoints)
        {
            patrolPositions.Clear();
            patrolPositions = new List<Vector3>(newPoints);
        }

        private void GotoNextPatrolPoint(bool loop)
        {
            // Returns if only the starting position is present
            if (patrolPositions.Count < 2) return;

            if (loop)
            {
                agent.SetDestination(patrolPositions[destinationCount % patrolPositions.Count]);
            }
            agent.SetDestination(patrolPositions[Utils.PingPong(destinationCount, patrolPositions.Count - 1)]);
            destinationCount++;
        }
    }
}


