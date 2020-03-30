﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPCs.Components;

public enum BrutusOfficeStates
{
    SittingIdle,
    SittingLookingAround,
    Responding,
    StandingWaiting,
    ReturningToDesk,
    RotateToDesk,
    Chasing
}

[RequireComponent(typeof(Chaser))]
[RequireComponent(typeof(Responder))]
[RequireComponent(typeof(Waiter))]
[RequireComponent(typeof(Patroller))]
public class AIBrutusOffice : NPCs.BaseStateAi<BrutusOfficeStates>
{
    public float standingWaitInterval = 3f;
    public float sittingAnimationLoopInterval = 5f;

    private Vector3 originPosition;
    private Quaternion originRotation;

    private Chaser chaser;
    private Responder responder;
    private Waiter waiter;
    private Patroller patroller;
    private Animator animator;

    protected override void Awake()
    {
        base.Awake();

        chaser = Utils.GetRequiredComponent<Chaser>(this);
        responder = Utils.GetRequiredComponent<Responder>(this);
        waiter = Utils.GetRequiredComponent<Waiter>(this);
        patroller = Utils.GetRequiredComponent<Patroller>(this);

        animator = Utils.GetRequiredComponentInChildren<Animator>(this);
    }

    public void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;

        chaser.OnSeePlayer += () => SetState(BrutusOfficeStates.Chasing);
        //chaser.OnLosePlayer += () => nextState = BrutusOfficeStates.StandingWaiting;
        chaser.OnReacquireTarget += () => SetState(BrutusOfficeStates.Chasing);
        responder.OnStartResponding += () => SetState(BrutusOfficeStates.Responding);
        patroller.OnRotationComplete += () => SetState(nextState);
        waiter.OnWaitComplete += () => SetState(nextState);

        patroller.SetPoints(new List<PatrolWaypoint>() { new PatrolWaypoint() { position = originPosition, rotation = originRotation.eulerAngles } });

        SetState(currentState);
        agent.enabled = true;
    }

    protected override void SetState(BrutusOfficeStates newState)
    {
        StopAllCoroutines();

        prevState = currentState;
        currentState = newState;

        switch (newState)
        {
            case BrutusOfficeStates.SittingIdle:
                SetSittingIdle();
                break;
            case BrutusOfficeStates.SittingLookingAround:
                SetSittingLookingAround();
                break;
            case BrutusOfficeStates.Responding:
                SetResponding();
                break;
            case BrutusOfficeStates.StandingWaiting:
                SetStandingWaiting();
                break;
            case BrutusOfficeStates.ReturningToDesk:
                SetReturningToDesk();
                break;
            case BrutusOfficeStates.RotateToDesk:
                SetRotateToDesk();
                break;
            case BrutusOfficeStates.Chasing:
                SetChasing();
                break;
            default:
                throw new UnityException("Invalid state name passed to " + GetType().Name);
        }
    }

    private void SetSittingIdle()
    {
        agent.isStopped = true;
        nextState = BrutusOfficeStates.SittingLookingAround;
        StartCoroutine(waiter.StartWaiting(sittingAnimationLoopInterval));
        AnimationSetStanding(false);
        AnimationSetMoving(false);
    }

    private void SetSittingLookingAround()
    {
        agent.isStopped = true;
        nextState = BrutusOfficeStates.SittingIdle;
        StartCoroutine(waiter.StartWaiting(sittingAnimationLoopInterval));
        AnimationSetStanding(false);
        AnimationSetMoving(false);
        AnimationLookAround();
    }

    private void SetResponding()
    {
        agent.isStopped = false;
        nextState = BrutusOfficeStates.StandingWaiting;
        agent.speed = responder.movementSpeed;
        agent.SetDestination(responder.responsePoint);
        AnimationSetStanding(true);
        AnimationSetMoving(true);
    }

    private void SetChasing()
    {
        agent.isStopped = false;
        nextState = BrutusOfficeStates.StandingWaiting;
        agent.speed = chaser.movementSpeed;
        agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
        AnimationSetStanding(true);
        AnimationSetMoving(true);
    }

    private void SetStandingWaiting()
    {
        agent.isStopped = true;
        StartCoroutine(waiter.StartWaiting(standingWaitInterval));
        AnimationSetStanding(true);
        AnimationSetMoving(false);
    }

    private void SetReturningToDesk()
    {
        agent.isStopped = false;
        agent.speed = patroller.movementSpeed;
        agent.SetDestination(patroller.GetNextPatrolWaypoint().position);
        AnimationSetStanding(true);
        AnimationSetMoving(true);
    }

    private void SetRotateToDesk()
    {
        agent.isStopped = true;
        StartCoroutine(patroller.StartRotating());
        AnimationSetStanding(true);
        AnimationSetMoving(false);
    }

    private void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh || agent.pathPending || agent.remainingDistance > agent.stoppingDistance) return;

        switch(currentState)
        {
            case BrutusOfficeStates.Chasing:
            case BrutusOfficeStates.Responding:
                nextState = BrutusOfficeStates.ReturningToDesk;
                SetState(BrutusOfficeStates.StandingWaiting);
                break;
            case BrutusOfficeStates.ReturningToDesk:
                nextState = BrutusOfficeStates.SittingIdle;
                SetState(BrutusOfficeStates.RotateToDesk);
                break;
        }
    }

    // ANIMATION FUNCTIONS
    private void AnimationLookAround()
    {
        animator.SetTrigger(Constants.ANIMATION_BRUTUSOFFICE_LOOKAROUND);
    }

    private void AnimationSetStanding(bool toggle)
    {
        animator.SetBool(Constants.ANIMATION_BRUTUSOFFICE_STANDING, toggle);
    }

    private void AnimationSetMoving(bool toggle)
    {
        animator.SetBool(Constants.ANIMATION_BRUTUSOFFICE_MOVING, toggle);
    }
}
