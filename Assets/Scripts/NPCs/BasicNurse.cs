using System;
using NPCs.Components;
using UnityEngine;

public enum BasicNurseStates
{
    Chasing,
    Responding,
    Searching,
    Patrolling,
    Rotating,
    Waiting,
}

namespace NPCs
{
    [RequireComponent(typeof(Chaser))]
    [RequireComponent(typeof(Responder))]
    [RequireComponent(typeof(Searcher))]
    [RequireComponent(typeof(Patroller))]
    [RequireComponent(typeof(Waiter))]
    public class BasicNurse : BaseStateAi<BasicNurseStates>
    {
        [HideInInspector] public Chaser chaser;
        [HideInInspector] public Responder responder;
        [HideInInspector] public Searcher searcher;
        [HideInInspector] public Patroller patroller;
        [HideInInspector] public Waiter waiter;

        public int numSearches = 3;
        public GameObject questionMark;
        private Animator animator;
        private int curNumSearches = 0;

        public override void Awake()
        {
            base.Awake();

            chaser = Utils.GetRequiredComponent<Chaser>(this);
            responder = Utils.GetRequiredComponent<Responder>(this);
            searcher = Utils.GetRequiredComponent<Searcher>(this);
            patroller = Utils.GetRequiredComponent<Patroller>(this);
            waiter = Utils.GetRequiredComponent<Waiter>(this);
            animator = Utils.GetRequiredComponentInChildren<Animator>(this);
        }

        public void Start()
        {
            chaser.OnSeePlayer += () => SetState(BasicNurseStates.Chasing);
            chaser.OnLosePlayer += () => nextState = BasicNurseStates.Searching; SetState(BasicNurseStates.Waiting);
            responder.OnStartResponding += () => SetState(BasicNurseStates.Responding);
            patroller.OnRotationComplete += () => SetState(BasicNurseStates.Waiting);
            waiter.OnWaitComplete += () => SetState(nextState);
            chaser.OnCollideWithPlayer += () => questionMark.SetActive(false);
            
            SetState(currentState);
        }

        protected override void SetState(BasicNurseStates newState)
        {
            switch (newState)
            {
                case BasicNurseStates.Chasing:
                    SetChasing();
                    break;
                case BasicNurseStates.Responding:
                    SetResponding();
                    break;
                case BasicNurseStates.Searching:
                    SetSearching();
                    break;
                case BasicNurseStates.Patrolling:
                    SetPatrolling();
                    break;
                case BasicNurseStates.Rotating:
                    SetRotating();
                    break;
                case BasicNurseStates.Waiting:
                    SetWaiting();
                    break;
                default:
                    questionMark.SetActive(false);
                    throw new UnityException("Invalid state name passed to " + GetType().Name);
            }
            
            currentState = newState;
        }

        private void SetChasing()
        {
            ToggleAnimations(true);
            agent.speed = chaser.movementSpeed;
            questionMark.SetActive(false);
            
            StopAllCoroutines();
            
            agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
        }

        private void SetResponding()
        {
            ToggleAnimations(true);
            agent.speed = responder.movementSpeed;
            questionMark.SetActive(true);
                    
            agent.SetDestination(responder.responsePoint);
            searcher.SetSearchPoint(responder.responsePoint);
        }

        private void SetSearching()
        {
            ToggleAnimations(true);
            agent.speed = searcher.movementSpeed;
            questionMark.SetActive(true);
            
            searcher.SetSearchPoint(agent.destination);
            agent.SetDestination(searcher.GetNextSearchPoint());
        }

        private void SetPatrolling()
        {
            ToggleAnimations(true);
            agent.speed = patroller.movementSpeed;
            questionMark.SetActive(false);
                    
            agent.SetDestination(patroller.GetNextPatrolWaypoint().position);
        }

        private void SetRotating()
        {
            ToggleAnimations(false);
            StartCoroutine(patroller.StartRotating());
        }

        private void SetWaiting()
        {
            ToggleAnimations(false);
            if (currentState == BasicNurseStates.Patrolling)
            {
                StartCoroutine(waiter.StartWaiting(patroller.curPatrolWaypoint.stayTime));
            }
            else
            {
                StartCoroutine(waiter.StartWaiting());
            }
        }

        protected void Update()
        {
            if (agent.hasPath || !agent.isOnNavMesh || agent.pathPending) return;

            // Choose the next state/behavior when the agent reaches current destination.
            switch (currentState)
            {
                case BasicNurseStates.Chasing:
                    nextState = BasicNurseStates.Searching;
                    SetState(BasicNurseStates.Waiting);
                    break;
                
                case BasicNurseStates.Responding:
                    SetState(BasicNurseStates.Searching);
                    break;
                
                case BasicNurseStates.Searching:
                    if (curNumSearches++ >= numSearches)
                    {
                        curNumSearches = 0;
                        nextState = BasicNurseStates.Patrolling;
                    }
                    else
                    {
                        nextState = BasicNurseStates.Searching;
                    }
                    SetState(BasicNurseStates.Waiting);
                    break;
                
                case BasicNurseStates.Patrolling:
                    nextState = BasicNurseStates.Patrolling;
                    SetState(BasicNurseStates.Rotating);
                    break;
                
                case BasicNurseStates.Rotating:
                    break;
                
                case BasicNurseStates.Waiting:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            if (currentState == BasicNurseStates.Chasing && chaser.isChasing)
            {
                agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
            }
        }

        private void ToggleAnimations(bool toggle)
        {
            // TODO: for whoever adds in walking or other animations here:
            // I was thinking that toggle=false just turns off all animations (disable the animator?)
            // and toggle=true can do specific behaivor based on currentState
            
            animator.SetBool(Constants.ANIMATION_STEVE_MOVING, toggle);
        }
    }
}
