using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NPCs.Components;

public enum TutorialNurseStates
{
    Idle,
    Chasing,
    Rotating,
    Patrolling,
    Waiting,
    Returning,
    ReturnThenIdle
}

namespace NPCs
{
    [RequireComponent(typeof(Chaser))]
    [RequireComponent(typeof(Patroller))]
    [RequireComponent(typeof(Waiter))]
    public class TutorialNurse : BaseStateAi<TutorialNurseStates>
    {
        [HideInInspector] public Chaser chaser;
        [HideInInspector] public Patroller patroller;
        [HideInInspector] public Waiter waiter;

        public PatrolWaypoint originWaypoint;
        public PatrolWaypoint checkBirdieWaypoint;

        private Animator anim;

        private Vector3 originPosition;
        private bool alwaysChasing = false;
        private bool foundBirdieAtLastWaypoint = true;

        public event System.Action OnCouldNotFindBirdie;

        protected override void Awake()
        {
            base.Awake();

            chaser = Utils.GetRequiredComponent<Chaser>(this);
            patroller = Utils.GetRequiredComponent<Patroller>(this);
            waiter = Utils.GetRequiredComponent<Waiter>(this);

            agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
            anim = Utils.GetRequiredComponentInChildren<Animator>(this);
        }

        private void Start()
        {
            originPosition = transform.position;

            chaser.OnReacquireTarget += () => { if (currentState == TutorialNurseStates.Chasing) agent.SetDestination(GameManager.Instance.GetPlayerTransform().position); };
            patroller.OnRotationComplete += () => SetState(TutorialNurseStates.Waiting);
            waiter.OnWaitComplete += () => SetState(nextState);

            patroller.SetPoints(new List<PatrolWaypoint>() { originWaypoint, checkBirdieWaypoint });
            SetState(TutorialNurseStates.Patrolling);
        }

        protected override void SetState(TutorialNurseStates newState)
        {
            switch (newState)
            {
                case TutorialNurseStates.Idle:
                    SetIdle();
                    break;
                case TutorialNurseStates.Chasing:
                    SetChasing();
                    break;
                case TutorialNurseStates.Patrolling:
                    SetPatrolling();
                    break;
                case TutorialNurseStates.Rotating:
                    SetRotating();
                    break;
                case TutorialNurseStates.Waiting:
                    SetWaiting();
                    break;
                case TutorialNurseStates.ReturnThenIdle:
                case TutorialNurseStates.Returning:
                    SetReturning();
                    break;
                default:
                    Debug.LogError($"Invalid state name passed to {name}");
                    break;
            }

            currentState = newState;
        }

        private void SetIdle()
        {
            StopAllCoroutines();
            ToggleAnimations(false);
            agent.enabled = false;
            alwaysChasing = false;
        }

        private void SetChasing()
        {
            StopAllCoroutines();
            agent.enabled = true;
            alwaysChasing = true;
        }

        private void SetPatrolling()
        {
            ToggleAnimations(true);
            agent.enabled = true;

            agent.SetDestination(patroller.GetNextPatrolWaypoint().position);

            if (patroller.curPatrolWaypoint == checkBirdieWaypoint)
            {
                foundBirdieAtLastWaypoint = false;
            }
            else if (patroller.curPatrolWaypoint == originWaypoint && !foundBirdieAtLastWaypoint)
            {
                OnCouldNotFindBirdie?.Invoke();
            }
        }

        private void SetRotating()
        {
            ToggleAnimations(false);
            StartCoroutine(patroller.StartRotating());
        }

        private void SetWaiting()
        {
            ToggleAnimations(false);
            if (currentState == TutorialNurseStates.Patrolling)
            {
                StartCoroutine(waiter.StartWaiting(patroller.curPatrolWaypoint.stayTime));
            }
            else
            {
                StartCoroutine(waiter.StartWaiting());
            }
        }

        private void SetReturning()
        {
            ToggleAnimations(true);
            agent.enabled = true;
            alwaysChasing = false;

            agent.SetDestination(originPosition);
        }

        public void SetLostBirdieWaypoints(List<PatrolWaypoint> waypoints)
        {
            patroller.SetPoints(waypoints);
            SetState(TutorialNurseStates.Patrolling);
        }

        // used by MissionTutorial to tell us we dont need to alert the nurse she's gone this time
        public void SetFoundBirdie()
        {
            foundBirdieAtLastWaypoint = true;
        }

        public void StartFollowingPlayer()
        {
            SetState(TutorialNurseStates.Chasing);
        }

        public void ReturnThenIdle()
        {
            SetState(TutorialNurseStates.ReturnThenIdle);
        }

        public void ReturnToOrigin()
        {
            SetState(TutorialNurseStates.Returning);
        }

        public void StopMovement()
        {
            // want them to keep rotating, but progress to idle instead of whatever they were planning to go to
            if (currentState == TutorialNurseStates.Rotating)
            {
                nextState = TutorialNurseStates.Idle;
            }
            // otherwise, just force to idle immediately
            else
            {
                SetState(TutorialNurseStates.Idle);
            }
        }

        protected void Update()
        {
            // make sure that even if the player leaves sight of the nurse, he still chases
            if (alwaysChasing)
            {
                chaser.isChasing = true;
            }

            // Used for states that need constant checking of some status
            switch (currentState)
            {
                case TutorialNurseStates.Chasing:
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        ToggleAnimations(false);
                    }
                    else
                    {
                        ToggleAnimations(true);
                    }
                    break;
            }

            if (!agent.isOnNavMesh || agent.pathPending || agent.remainingDistance > agent.stoppingDistance) return;

            // Choose the next state/behavior when the agent reaches current destination
            switch (currentState)
            {
                case TutorialNurseStates.Idle:
                    break;

                case TutorialNurseStates.Patrolling:
                    nextState = TutorialNurseStates.Patrolling;
                    SetState(TutorialNurseStates.Rotating);
                    break;

                case TutorialNurseStates.ReturnThenIdle:
                    SetState(TutorialNurseStates.Idle);
                    break;

                case TutorialNurseStates.Returning:
                    SetState(TutorialNurseStates.Patrolling);
                    break;

                case TutorialNurseStates.Rotating:
                    break;

                case TutorialNurseStates.Waiting:
                    break;
            }
        }

        private void ToggleAnimations(bool toggle)
        {
            // TODO: for whoever adds in walking or other animations here:
            // I was thinking that toggle=false just turns off all animations (disable the animator?)
            // and toggle=true can do specific behaivor based on currentState

            anim.SetBool(Constants.ANIMATION_STEVE_MOVING, toggle);
        }
    }
}
