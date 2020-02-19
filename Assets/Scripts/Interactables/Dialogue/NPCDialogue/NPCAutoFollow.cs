using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAutoFollow : DialogueAutoplay
{
    private bool isFollowing = false;
    protected GameObject targetObject;

    protected NavMeshAgent agent;


    void Start()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
    }


    void Update()
    {
        if(isFollowing)
        {
            // TODO: only chase character within a certain area
            ChaseTarget();
        }
    }

    // Temporary way to trigger interaction (Remove once Trigger Interaction is integrated)
    // Remove this and box collider on NPC when done
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            TriggerInteraction(other.gameObject);
        }
    }

    public new void TriggerInteraction(GameObject target)
    {
        base.TriggerInteraction(target);

        targetObject = target;
        TriggerFollow();
    }


    void TriggerFollow()
    {
        if(targetObject == null)
        {
            Debug.LogError("Target to follow must be assigned");
            return;
        }

        isFollowing = true;
    }


    void StopFollow()
    {
        isFollowing = false;
    }


    protected override void OnDialogueComplete()
    {
        base.OnDialogueComplete();
        StopFollow();
        // Code to add mission can possibly be put here
    }


    void ChaseTarget()
    {
        if (targetObject == null)
        {
            Debug.LogError("Object being followed must be assigned");
            return;
        }
        else
        {
            agent.SetDestination(targetObject.transform.position);
        }
    }
}
