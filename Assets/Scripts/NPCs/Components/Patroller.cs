using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [RequireComponent(typeof(BaseAi))]
    public class Patroller : MonoBehaviour
    {
        public float movementSpeed = 6.0f;
        public float rotationSpeed = 180.0f;
        
        private List<Vector3> patrolPositions = new List<Vector3>();
        private List<Quaternion> patrolRotations = new List<Quaternion>();
        private List<float> patrolStayTimes = new List<float>();
        
        private int destinationCount;
        private Quaternion curRotation;
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
            patrolRotations = newRotations.Select(i => Quaternion.Euler(i.x, i.y, i.z)).ToList();
            patrolStayTimes = new List<float>(newStayTimes);

            if (patrolPositions.Count != patrolRotations.Count  || patrolPositions.Count != patrolStayTimes.Count)
            {
                throw new UnityException("Patroller passed waypoint information of differing lengths");
            }
            
            curRotation = patrolRotations[0];
            curStayTime = patrolStayTimes[0];
        }

        public bool RotationComplete()
        {
            if (transform.rotation == curRotation)
            {
                baseAi.agent.isStopped = false;
                return true;
            }

            baseAi.agent.isStopped = true;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, curRotation, Time.deltaTime * rotationSpeed);
            return false;
        }

        public void GotoNextPatrolPoint(bool loop=true)
        {
            // Returns if only the starting position is present
            if (patrolPositions.Count < 2) return;

            int newIndex;
            if (loop) newIndex = destinationCount % patrolPositions.Count;
            else newIndex = Utils.PingPong(destinationCount, patrolPositions.Count - 1);

            agent.SetDestination(patrolPositions[newIndex]);
            curRotation = patrolRotations[newIndex];
            curStayTime = patrolStayTimes[newIndex];

            destinationCount++;
        }
    }
}


