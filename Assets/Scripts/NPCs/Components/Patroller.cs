using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Patroller : MonoBehaviour
    {
        public float movementSpeed = 6.0f;
        public List<Vector3> patrolPositions = new List<Vector3>();
        public List<Vector3> patrolRotations = new List<Vector3>();
        public List<float> patrolStayTimes = new List<float>();
        
        private int destinationCount;
        [HideInInspector] public float curStayTime = 1.0f;
        
        private BaseAi baseAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
        }

        public void SetPoints(List<Vector3> newPositions, List<Vector3> newRotations, List<float> newStayTimes)
        {
            patrolPositions.Clear();
            patrolRotations.Clear();
            patrolStayTimes.Clear();
            
            patrolPositions = new List<Vector3>(newPositions);
            patrolRotations = new List<Vector3>(newRotations);
            patrolStayTimes = new List<float>(newStayTimes);
        }

        public void GotoNextPatrolPoint(bool loop=true)
        {
            // Returns if only the starting position is present
            if (patrolPositions.Count < 2) return;

            int newIndex;
            if (loop) newIndex = destinationCount % patrolPositions.Count;
            else newIndex = Utils.PingPong(destinationCount, patrolPositions.Count - 1);

            agent.SetDestination(patrolPositions[newIndex]);
            curStayTime = patrolStayTimes[newIndex];
            
            destinationCount++;
        }
    }
}


