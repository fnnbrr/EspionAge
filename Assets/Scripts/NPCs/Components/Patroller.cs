using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace NPCs.Components
{
    [System.Serializable]
    public class PatrolWaypoint
    {
        public Vector3 position;
        public Vector3 rotation;
        public float stayTime;
    }
    
    [RequireComponent(typeof(BaseAi))]
    public class Patroller : MonoBehaviour
    {
        public float movementSpeed = 6.0f;
        public float rotationSpeed = 180.0f;

        private List<PatrolWaypoint> patrolWaypoints;

        private int destinationCount;
        private Quaternion curRotation;
        [HideInInspector] public float curStayTime = 1.0f;

        private BaseAi baseAi;
        private NavMeshAgent agent;

        private void Awake()
        {
            baseAi = Utils.GetRequiredComponent<BaseAi>(this);
            agent = baseAi.agent;
            
            patrolWaypoints = new List<PatrolWaypoint>();
        }

        public void SetPoints(List<PatrolWaypoint> newWaypoints)
        {
            patrolWaypoints = newWaypoints;

            baseAi.agent.SetDestination(patrolWaypoints[0].position);
            curRotation = Quaternion.Euler(patrolWaypoints[0].rotation);
            curStayTime = patrolWaypoints[0].stayTime;
        }

        public bool RotationComplete()
        {
            if (transform.rotation == curRotation)
            {
                baseAi.ToggleAnimations(true);
                return true;
            }

            baseAi.ToggleAnimations(false);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, curRotation, Time.deltaTime * rotationSpeed);
            return false;
        }

        public void GotoNextPatrolPoint(bool loop=true)
        {
            // Returns if only the starting position is present
            if (patrolWaypoints.Count < 2) return;

            int newIndex;
            if (loop)
            {
                newIndex = destinationCount % patrolWaypoints.Count;
            }
            else
            {
                newIndex = Utils.PingPong(destinationCount, patrolWaypoints.Count - 1);
            }

            baseAi.agent.SetDestination(patrolWaypoints[newIndex].position);
            curRotation = Quaternion.Euler(patrolWaypoints[newIndex].rotation);
            curStayTime = patrolWaypoints[newIndex].stayTime;

            destinationCount++;
        }
    }
}


