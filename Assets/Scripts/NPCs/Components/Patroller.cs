using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Patroller : MonoBehaviour
    {
        public Transform patrolWaypoints;
        
        private BaseAi baseAi;
        private NavMeshAgent agent;
        public List<Vector3> patrolPositions = new List<Vector3>();
        private int destinationCount;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
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

        public void GotoNextPatrolPoint(bool loop=true)
        {
            // Returns if only the starting position is present
            if (patrolPositions.Count < 2) return;

            if (loop)
            {
                agent.SetDestination(patrolPositions[destinationCount % patrolPositions.Count]);
            }
            else
            {
                agent.SetDestination(patrolPositions[Utils.PingPong(destinationCount, patrolPositions.Count - 1)]);
            }
            
            destinationCount++;
        }
    }
}


