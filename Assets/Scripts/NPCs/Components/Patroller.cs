using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCs.Components
{
    [Serializable]
    public class PatrolWaypoint
    {
        public Vector3 position;
        public Vector3 rotation;
        public float stayTime;
    }
    
    public class Patroller : MonoBehaviour
    {
        public float movementSpeed = 6.0f;
        public float rotationSpeed = 180.0f;

        private List<PatrolWaypoint> patrolWaypoints;
        public PatrolWaypoint curPatrolWaypoint;

        private int destinationCount;
        
        public event Action OnRotationComplete;

        public void Awake()
        {
            // Allows a Patroller given no PatrolWaypoints to simply stay in place
            SetDefaultCurPatrolWaypoint();
        }

        public void SetDefaultCurPatrolWaypoint()
        {
            curPatrolWaypoint = new PatrolWaypoint
            {
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                stayTime = 10.0f
            };
        }

        public void SetPoints(List<PatrolWaypoint> newWaypoints)
        {
            if (newWaypoints.Count < 1)
            {
                throw new UnityException("Cannot set Patroller waypoints using an empty list");
            }
            
            patrolWaypoints = newWaypoints;
        }

        public IEnumerator StartRotating()
        {
            Quaternion curRotation = Quaternion.Euler(curPatrolWaypoint.rotation);

            while (transform.rotation != curRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, curRotation, 
                    Time.fixedDeltaTime * rotationSpeed);
                yield return new WaitForSeconds(Time.fixedDeltaTime);
            }
            
            OnRotationComplete?.Invoke();
        }

        public PatrolWaypoint GetNextPatrolWaypoint(bool loop=true)
        {
            if (patrolWaypoints == null)
            {
                return curPatrolWaypoint;
            }
            
            int newIndex;
            if (loop)
            {
                newIndex = destinationCount % patrolWaypoints.Count;
            }
            else
            {
                newIndex = Utils.PingPong(destinationCount, patrolWaypoints.Count - 1);
            }
            
            destinationCount++;
            
            curPatrolWaypoint = patrolWaypoints[newIndex];
            return curPatrolWaypoint;
        }
    }
}


