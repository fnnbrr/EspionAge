using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAutoFollow : DialogueAutoplay
{
    private bool isFollowing = false;
    protected GameObject targetObject;

    protected NavMeshAgent agent;

    protected override void Start()
    {
        base.Start();
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
    }

    void Update()
    {
        if(isFollowing)
        {
            // TODO: only chase character is not within range of the target
            ChaseTarget();
        }
        // TODO: set a way for the character to stop chasing the target
    }

    //TEMPORARY TO TEST
    public new void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            TriggerAutoplay();
            TriggerFollow(other.gameObject);
        }
    }


    public void TriggerFollow(GameObject _targetObject)
    {
        if(!isFollowing)
        {
            isFollowing = true;
            targetObject = _targetObject;
        }
    }

    // TODO: Function that handles when autoplay of text is happening and if it should start following the target
    protected override void InactivateDialogue()
    {
        base.InactivateDialogue();
        isFollowing = false;
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
            //Chase target
            Vector3 targetPosition = targetObject.transform.position;
            Vector3 thisPosition = transform.position;
            Vector3 dirToTarget = thisPosition - targetPosition;
            Vector3 newPos = thisPosition - dirToTarget;

            agent.SetDestination(newPos);
        }
    }
}
