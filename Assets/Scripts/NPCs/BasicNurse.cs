using System;
using System.Collections.Generic;
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

        public int numSearches = 3;
        private int curNumSearches = 0;
        
        private GameObject questionMark;
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

            questionMark = transform.Find("QuestionMark").gameObject;
            
            SetState(currentState);
        }

        public override void SetState(string newState)
        {
            switch (newState)
            {
                case "Searching":
                    agent.isStopped = false;
                    agent.speed = searcher.movementSpeed;
                    questionMark.SetActive(true);
                    break;
                case "Responding":
                    agent.isStopped = false;
                    agent.speed = responder.movementSpeed;
                    questionMark.SetActive(true);
                    break;
                case "Chasing":
                    agent.isStopped = false;
                    agent.speed = chaser.movementSpeed;
                    questionMark.SetActive(false);
                    break;
                case "Patrolling":
                    agent.isStopped = false;
                    agent.speed = patroller.movementSpeed;
                    questionMark.SetActive(false);
                    break;
                default:
                    questionMark.SetActive(false);
                    throw new UnityException("Invalid state name passed to " + GetType().Name);
            }
        
            currentState = newState;
        }

        protected override void Update()
        {
            animator.SetBool(Constants.ANIMATION_STEVE_MOVING, !agent.isStopped);
            
            if (!agent.isOnNavMesh || agent.pathPending || agent.remainingDistance > 0.5f) return;

            // Choose the next state/behavior when the agent gets close to the current destination.
            switch (currentState)
            {
                case "Patrolling":
                    if (!patroller.RotationComplete()) break;
                    if (!waiter.WaitComplete(patroller.curStayTime)) break;
                    patroller.GotoNextPatrolPoint();
                    break;
                case "Chasing":
                    SetState("Searching");
                    break;
                case "Responding":
                    SetState("Searching");
                    break;
                case "Searching":
                    if (!waiter.WaitComplete()) break;
                    if (curNumSearches < numSearches)
                    {
                        curNumSearches += 1;
                        searcher.GotoNextSearchPoint();
                    }
                    else
                    {
                        curNumSearches = 0;
                        SetState("Patrolling");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
