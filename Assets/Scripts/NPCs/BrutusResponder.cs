using System;
using NPCs.Components;
using UnityEngine;

public enum BrutusResponderStates
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
    [RequireComponent(typeof(Enemy))]
    public class BrutusResponder : BaseStateAi<BrutusResponderStates>
    {
        [HideInInspector] public Chaser chaser;
        [HideInInspector] public Responder responder;
        [HideInInspector] public Searcher searcher;
        [HideInInspector] public Patroller patroller;
        [HideInInspector] public Waiter waiter;
        [HideInInspector] public Enemy enemy;

        public int numSearches = 3;
        public GameObject questionMark;
        private Animator animator;
        private int curNumSearches = 0;

        protected override void Awake()
        {
            base.Awake();

            chaser = Utils.GetRequiredComponent<Chaser>(this);
            responder = Utils.GetRequiredComponent<Responder>(this);
            searcher = Utils.GetRequiredComponent<Searcher>(this);
            patroller = Utils.GetRequiredComponent<Patroller>(this);
            waiter = Utils.GetRequiredComponent<Waiter>(this);
            animator = Utils.GetRequiredComponentInChildren<Animator>(this);
            enemy = Utils.GetRequiredComponent<Enemy>(this);
        }

        public void Start()
        {
            chaser.OnSeePlayer += () => SetState(BrutusResponderStates.Chasing);
            chaser.OnLosePlayer += () => nextState = BrutusResponderStates.Searching; SetState(BrutusResponderStates.Waiting);
            chaser.OnReacquireTarget += () => agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
            responder.OnStartResponding += () => SetState(BrutusResponderStates.Responding);
            patroller.OnRotationComplete += () => SetState(BrutusResponderStates.Waiting);
            waiter.OnWaitComplete += () => SetState(nextState);
            enemy.OnCollideWithPlayer += () => questionMark.SetActive(false);
            
            SetState(currentState);
        }

        protected override void SetState(BrutusResponderStates newState)
        {
            prevState = currentState;
            currentState = newState;
            
            switch (newState)
            {
                case BrutusResponderStates.Chasing:
                    SetChasing();
                    break;
                case BrutusResponderStates.Responding:
                    SetResponding();
                    break;
                case BrutusResponderStates.Searching:
                    SetSearching();
                    break;
                case BrutusResponderStates.Patrolling:
                    SetPatrolling();
                    break;
                case BrutusResponderStates.Rotating:
                    SetRotating();
                    break;
                case BrutusResponderStates.Waiting:
                    SetWaiting();
                    break;
                default:
                    questionMark.SetActive(false);
                    throw new UnityException("Invalid state name passed to " + GetType().Name);
            }
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
            if (prevState == BrutusResponderStates.Patrolling)
            {
                StartCoroutine(waiter.StartWaiting(patroller.curPatrolWaypoint.stayTime));
            }
            else
            {
                StartCoroutine(waiter.StartWaiting());
            }
        }

        private void CheckNursesAlertBrutus()
        {
            // Handle Brutus-specific behavior of responding to player once seen by any nurse
            if (currentState == BrutusResponderStates.Chasing || !GameManager.Instance.isPlayerSpotted) return;
            
            responder.responsePoint = GameManager.Instance.GetPlayerTransform().position;
            agent.SetDestination(responder.responsePoint);
                
            if (currentState != BrutusResponderStates.Responding)
            {
                SetState(BrutusResponderStates.Responding);
            }
        }

        protected void Update()
        {
            CheckNursesAlertBrutus();
            
            if (agent.hasPath || !agent.isOnNavMesh || agent.pathPending) return;

            // Choose the next state/behavior when the agent reaches current destination.
            switch (currentState)
            {
                case BrutusResponderStates.Chasing:
                    nextState = BrutusResponderStates.Searching;
                    SetState(BrutusResponderStates.Waiting);
                    break;
                
                case BrutusResponderStates.Responding:
                    SetState(BrutusResponderStates.Searching);
                    break;
                
                case BrutusResponderStates.Searching:
                    if (curNumSearches++ >= numSearches)
                    {
                        curNumSearches = 0;
                        nextState = BrutusResponderStates.Patrolling;
                    }
                    else
                    {
                        nextState = BrutusResponderStates.Searching;
                    }
                    SetState(BrutusResponderStates.Waiting);
                    break;
                
                case BrutusResponderStates.Patrolling:
                    nextState = BrutusResponderStates.Patrolling;
                    SetState(BrutusResponderStates.Rotating);
                    break;
                
                case BrutusResponderStates.Rotating:
                    break;
                
                case BrutusResponderStates.Waiting:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ToggleAnimations(bool toggle)
        {
            // TODO: implement once Brutus animations are ready
        }
    }
}
