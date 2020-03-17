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
                    agent.speed = searcher.movementSpeed;
                    questionMark.SetActive(true);
                    break;
                case "Responding":
                    agent.speed = responder.movementSpeed;
                    questionMark.SetActive(true);
                    break;
                case "Chasing":
                    agent.speed = chaser.movementSpeed;
                    questionMark.SetActive(false);
                    break;
                case "Patrolling":
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

        public override void ToggleAnimations(bool toggle)
        {
            // TODO: for whoever adds in walking or other animations here:
            // I was thinking that toggle=false just turns off all animations (disable the animator?)
            // and toggle=true can do specific behaivor based on currentState
            
            animator.SetBool(Constants.ANIMATION_STEVE_MOVING, toggle);
        }
    }
}
