using System;
using System.Collections.Generic;
using FMOD.Studio;
using NPCs.Components;
using UnityEngine;

namespace NPCs
{
    [RequireComponent(typeof(Chaser))]
    [RequireComponent(typeof(Responder))]
    [RequireComponent(typeof(Searcher))]
    [RequireComponent(typeof(Patroller))]
    [RequireComponent(typeof(Waiter))]
    [RequireComponent(typeof(Animator))]
    public class BasicNurse : BaseAi
    {
        private static readonly HashSet<string> States = new HashSet<string>()
        {
            "Chasing",
            "Responding",
            "Searching",
            "Patrolling",
        };

        [HideInInspector] public Chaser chaser;
        [HideInInspector] public Responder responder;
        [HideInInspector] public Searcher searcher;
        [HideInInspector] public Patroller patroller;
        [HideInInspector] public Waiter waiter;

        public int numSearches = 5;
        private int curNumSearches = 0;
        
        public GameObject questionMark;
        private Animator animator;

        private new void Awake()
        {
            base.Awake();

            chaser = Utils.GetRequiredComponent<Chaser>(this);
            responder = Utils.GetRequiredComponent<Responder>(this);
            searcher = Utils.GetRequiredComponent<Searcher>(this);
            patroller = Utils.GetRequiredComponent<Patroller>(this);
            waiter = Utils.GetRequiredComponent<Waiter>(this);
            animator = Utils.GetRequiredComponentInChildren<Animator>(this);
        }

        public override void SetState(string newState)
        {
            switch (newState)
            {
                case "Searching":
                    ShowQuestionMark();
                    break;
                case "Responding":
                    ShowQuestionMark();
                    break;
                case "Chasing":
                    HideQuestionMark();
                    break;
                case "Patrolling":
                    HideQuestionMark();
                    break;
                default:
                    HideQuestionMark();
                    throw new UnityException("Invalid state name passed to " + GetType().Name);
            }
        
            currentState = newState;
        }

        protected override void Update()
        {
            animator.SetBool(Constants.ANIMATION_STEVE_MOVING, !agent.isStopped);
            
            if (!agent.isOnNavMesh || agent.pathPending)
            {
                animator.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
                return;
            }

            if (agent.remainingDistance > 0.5f)
            {
                animator.SetBool(Constants.ANIMATION_STEVE_MOVING, true);
                return;
            }

            // Choose the next destination point when the agent gets close to the current one.
            switch (currentState)
            {
                case "Patrolling":
                    if (waiter.WaitComplete())
                    {
                        patroller.GotoNextPatrolPoint();
                    }
                    break;
                case "Chasing":
                    chaser.ChaseTarget();
                    break;
                case "Responding":
                    if (waiter.WaitComplete())
                    {
                        SetState("Searching");
                    }
                    break;
                case "Searching":
                    if (waiter.WaitComplete())
                    {
                        if (curNumSearches < numSearches)
                        {
                            curNumSearches += 1;
                            searcher.GotoNextSearchPoint();
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

        private void ShowQuestionMark()
        {
            questionMark.SetActive(true);
        }

        private void HideQuestionMark()
        {
            questionMark.SetActive(false);
        }
    }
}
