using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCs
{
    public class Patroller : BaseAi
    {
        [Header("Patrolling")]
        public Transform patrolWaypoints;
        [Tooltip("After responding, how many times should I search before resuming my patrol?")]
        public int numSearches = 2;

        private int curNumSearches = 0;

        private List<Vector3> points = new List<Vector3>();
        private int destinationCount;

        private void Awake()
        {
            defaultState = ActionStates.Patrolling;
            Animator = Utils.GetRequiredComponentInChildren<Animator>(this);
        }

        // Start is called before the first frame update
        new void Start()
        {
            base.Start();

            if (patrolWaypoints)
            {
                foreach (Transform childWaypoint in patrolWaypoints)
                {
                    points.Add(childWaypoint.position);
                }
                agent.SetDestination(points[0]);
            }
        }

        public void SetPoints(List<Vector3> newPoints)
        {
            points.Clear();
            points = new List<Vector3>(newPoints);
        }

        // Cycles through points start->end, then end->start
        private void GotoNextPatrolPoint()
        {
            // Returns if only the starting position is present
            if (points.Count < 2) return;

            // Set the agent to go to the currently selected destination.
            agent.SetDestination(points[Utils.PingPong(destinationCount, points.Count - 1)]);
            destinationCount++;
        }

        protected override void Update()
        {
            // TEMP, we gotta refactor this logic to make this work better and be more robust
            if (IsWaiting)
            {
                Animator.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
                WaitTimer -= Time.deltaTime;
                if (WaitTimer <= 0f)
                {
                    agent.enabled = true;
                    IsWaiting = false;
                }
                return;
            }

            if (!agent.isOnNavMesh || agent.pathPending)
            {
                Animator.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
                return;
            }

            if (agent.remainingDistance > 0.5f)
            {
                Animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);
                return;
            }

            // Choose the next destination point when the agent gets close to the current one.
            switch (currentState)
            {
                case ActionStates.Patrolling:
                    if (WaitComplete())
                    {
                        GotoNextPatrolPoint();
                    }
                    break;
                case ActionStates.Chasing:
                    ChaseTarget();
                    break;
                case ActionStates.Responding:
                    if (WaitComplete())
                    {
                        SetState(ActionStates.Searching);
                    }
                    break;
                case ActionStates.Searching:
                    if (WaitComplete())
                    {
                        if (curNumSearches < numSearches)
                        {
                            curNumSearches += 1;
                            GotoNextSearchPoint();
                        }
                        else
                        {
                            curNumSearches = 0;
                            SetState(defaultState);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
