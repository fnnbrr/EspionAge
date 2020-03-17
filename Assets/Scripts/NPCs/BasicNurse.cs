using System;
using NPCs.Components;
using UnityEngine;

public enum BasicNurseStates
{
    Chasing,
    Responding,
    Searching,
    Patrolling,
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
        private int curNumSearches = 0;
        
        public GameObject questionMark;
        private Animator animator;

        public override void Awake()
        {
            base.Awake();

            chaser = Utils.GetRequiredComponent<Chaser>(this);
            responder = Utils.GetRequiredComponent<Responder>(this);
            searcher = Utils.GetRequiredComponent<Searcher>(this);
            patroller = Utils.GetRequiredComponent<Patroller>(this);
            waiter = Utils.GetRequiredComponent<Waiter>(this);
            animator = Utils.GetRequiredComponentInChildren<Animator>(this);
            
            SetState(currentState);
        }

        public void Start()
        {
            chaser.OnSeePlayer += () => SetState(BasicNurseStates.Chasing);
            chaser.OnLosePlayer += () => SetState(BasicNurseStates.Searching);
            responder.OnStartResponding += () => SetState(BasicNurseStates.Responding);
        }

        protected override void SetState(BasicNurseStates newState)
        {
            switch (newState)
            {
                case BasicNurseStates.Searching:
                    agent.speed = searcher.movementSpeed;
                    searcher.searchPosition = agent.destination;
                    questionMark.SetActive(true);
                    break;
                case BasicNurseStates.Responding:
                    agent.speed = responder.movementSpeed;
                    agent.SetDestination(responder.responsePoint);
                    searcher.SetSearchPoint(responder.responsePoint);
                    questionMark.SetActive(true);
                    break;
                case BasicNurseStates.Chasing:
                    agent.speed = chaser.movementSpeed;
                    agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
                    questionMark.SetActive(false);
                    break;
                case BasicNurseStates.Patrolling:
                    agent.speed = patroller.movementSpeed;
                    questionMark.SetActive(false);
                    break;
                default:
                    questionMark.SetActive(false);
                    throw new UnityException("Invalid state name passed to " + GetType().Name);
            }
            
            currentState = newState;
            ToggleAnimations(true);
        }

        protected override void Update()
        {
            if (agent.hasPath || !agent.isOnNavMesh || agent.pathPending) return;

            // Choose the next state/behavior when the agent reaches current destination.
            switch (currentState)
            {
                case BasicNurseStates.Patrolling:
                    if (!patroller.RotationComplete()) break;
                    if (!waiter.WaitComplete(patroller.curStayTime)) break;
                    patroller.GotoNextPatrolPoint();
                    break;
                case BasicNurseStates.Chasing:
                    SetState(BasicNurseStates.Searching);
                    break;
                case BasicNurseStates.Responding:
                    SetState(BasicNurseStates.Searching);
                    break;
                case BasicNurseStates.Searching:
                    if (!waiter.WaitComplete()) break;
                    if (curNumSearches < numSearches)
                    {
                        curNumSearches += 1;
                        agent.destination = searcher.GetNextSearchPoint();
                    }
                    else
                    {
                        curNumSearches = 0;
                        SetState(BasicNurseStates.Patrolling);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
