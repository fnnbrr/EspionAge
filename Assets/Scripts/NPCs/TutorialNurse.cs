using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialNurse : MonoBehaviour
{
    public float rotationSpeed = 1f;

    private NavMeshAgent agent;
    private FieldOfVision fov;
    private Animator anim;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private bool followPlayer = false;
    private bool rotatedToOrigin = true;

    private void Awake()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        fov = Utils.GetRequiredComponent<FieldOfVision>(this);
        anim = Utils.GetRequiredComponentInChildren<Animator>(this);
        fov.OnTargetSpotted += HandleOnPlayerSpotted;
    }

    private void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
    }

    private void HandleOnPlayerSpotted()
    {
        followPlayer = true;
        rotatedToOrigin = false;
    }

    public void StopFollowingPlayer()
    {
        followPlayer = false;
        agent.SetDestination(originPosition);
    }

    private void FixedUpdate()
    {
        if (followPlayer)
        {
            agent.SetDestination(GameManager.Instance.GetPlayerTransform().position);
            anim.SetBool(Constants.ANIMATION_STEVE_MOVING, true);
        }
        else if (!rotatedToOrigin)
        {
            if (agent.remainingDistance <= 0.5f)
            {
                RotateBackToOrigin();
                anim.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
            }
            else
            {
                anim.SetBool(Constants.ANIMATION_STEVE_MOVING, true);
            }
        } 
        else
        {
            anim.SetBool(Constants.ANIMATION_STEVE_MOVING, false);
        }
    }

    private void RotateBackToOrigin()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, originRotation, Time.deltaTime * rotationSpeed);
        if (transform.rotation.Equals(originRotation))
        {
            rotatedToOrigin = true;
        }
    }
}
