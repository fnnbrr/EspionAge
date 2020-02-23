using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAction : MonoBehaviour
{
    protected NPCInteractable npcInteractScript;

    protected NavMeshAgent agent;
    protected GameObject targetObject;

    public bool isFollowing = false;
    private Vector3 originPosition;
    private Transform playerTransform;

    public bool withinRadius = false;
    public bool checkingRadius = true;
    public float boundaryRadius = 12.0f;


    void Start()
    {
        agent = Utils.GetRequiredComponent<NavMeshAgent>(this);
        originPosition = gameObject.transform.position;
        npcInteractScript = Utils.GetRequiredComponent<NPCInteractable>(gameObject);
        playerTransform = GameManager.Instance.GetPlayerTransform();
    }

    // Update is called once per frame
    void Update()
    {
        if (checkingRadius)
        {
            if(IsWithinRadius(playerTransform) && !withinRadius)
            {
                withinRadius = true;
                TriggerFollow(playerTransform.gameObject);
                npcInteractScript.StartAutoplayInteraction();
            }
        }

        if (isFollowing)
        {
            FollowTarget();
        }

        if (!IsWithinRadius(playerTransform))
        {
            withinRadius = false;
            checkingRadius = false;
            StopFollow();
            ReturnToOrigin();
        }
    }

    void FollowTarget()
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

    private bool IsWithinRadius(Transform position)
    {
        Debug.Log(Vector3.Distance(originPosition, position.position));
        return Vector3.Distance(originPosition, position.position) < boundaryRadius;
    }

    public void SetOriginPosition(Vector3 position)
    {
        originPosition = position;
    }


    public void ReturnToOrigin()
    {
        agent.SetDestination(originPosition);
    }


    protected void TriggerFollow(GameObject target)
    {
        targetObject = target;

        if (targetObject == null)
        {
            Debug.LogError("Target to follow must be assigned");
            return;
        }

        isFollowing = true;
    }


    protected void StopFollow()
    {
        isFollowing = false;
    }
}
