using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    protected GameObject player;

    // Must attach in instances in the inspector
    public Animator interactableAnim;
    public RectTransform interactTransform;

    protected bool enableInteract = true;
    protected bool interactableOn = false;
    protected bool withinInteractRadius;

    public float interactRadius = Constants.INTERACT_POPUP_RADIUS;

    public delegate void OnInteractEventHandler(Interactable source);
    public event OnInteractEventHandler OnInteractEnd;

    protected virtual void Start()
    {
        player = GameManager.Instance.GetPlayerTransform().gameObject;
    }

    protected virtual void Update()
    {
        if (enableInteract)
        {
            withinInteractRadius = IsWithinRadius(transform.position, player.transform, interactRadius);

            if (!interactableOn && withinInteractRadius)
            {
                // Do a raycast to see if there is anything that is obstructing the object from the player
                // Solves issue of being able to talk through a wall
                RaycastHit hit;
                if (Physics.Raycast(player.transform.position, (transform.position - player.transform.position),
                    out hit, interactRadius))
                {
                    if (!hit.collider.gameObject.Equals(this.gameObject))
                    { 
                        return;
                    }
                }

                interactableOn = true;
                ShowInteractUI();
            }
            else if(interactableOn && !withinInteractRadius)
            {
                interactableOn = false;
                HideInteractUI();
            }

            //Ensures text is always facing the camera
            if (interactableOn)
            {
                // Interact Text always faces towards camera
                interactTransform.LookAt(CameraManager.Instance.GetActiveCameraTransform());

                // User chooses to interact with the item
                if (Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
                {
                    //TODO: Get FaceInteractable to work and not freeze player rotation
                    //FaceInteractable();
                    HideInteractUI();
                    OnInteract();
                    interactableOn = false;
                }
            }
        }
    }

    // Handle the dialogue for this interactable
    public virtual void OnInteract()
    {
        if (!GameManager.Instance.GetPlayerController().EnablePlayerInput) return;
        
        // Start Dialogue
        // We cannot access the OnInteractEnd event from a derived class, must do this:
        // https://l.messenger.com/l.php?u=https%3A%2F%2Fstackoverflow.com%2Fquestions%2F4496799%2Fevent-can-only-appear-on-the-left-hand-side-of-or&h=AT00_XXqKtlm4qXzLotgXnPjPO2iqiYBcQtVkl_JrXtqEMBYJjmljCKHzEILlPthSMNjJjKHvn4aRXioteTvEvlrFo3wdk8hteIEUndnUco_OVnh6qiDClK7Hf7rkeyT_-oYgI1-21-b4yIuh22-2rkt
        // Signal interact end after the dialogue is handled
        OnInteractEnd?.Invoke(this);
    }

    // Changes the rotation of the player to face the interactable object
    public void FaceInteractable()
    {
        // Ensures player game object has been assigned
        if (player is null)
        {
            Debug.LogError("Player reference not found");
            return;
        }

        Vector3 dirToFace = transform.position - player.transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        // Animation of facing the interactable
        // Possible issue to prevent another coroutine being called if player is already rotating
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        StartCoroutine(RotateAnimation(player, rotation, GameManager.Instance.GetMovementController().turnSpeed, () => UnfreezePlayer()));

    }

    // Coroutine that animates the rotation of the given object to the desiredRotation at a set turn speed
    protected IEnumerator RotateAnimation(GameObject obj, Quaternion desiredRotation, float turnSpeed, Action onFinishCallback = null)
    {
        Quaternion startRotation = obj.transform.rotation;

        float t = 0;
    
        while (Mathf.Abs(Mathf.DeltaAngle(obj.transform.eulerAngles.y, desiredRotation.eulerAngles.y)) > 1.0f)
        {
            obj.transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, t);
            t += Time.deltaTime * turnSpeed;

            yield return null;
        }

        // Prevents the user from rotating/moving while doing the rotation animation
        onFinishCallback?.Invoke();
    }

    protected bool IsWithinRadius(Vector3 center, Transform targetTransform, float radius)
    {
        return Vector3.Distance(center, targetTransform.position) < radius;
    }

    private void UnfreezePlayer()
    {
        if (player != null)
        {
            Debug.LogError("Trying to access a player that has not been assigned");
        }

        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
    }

    protected void ShowInteractUI()
    {
        interactableAnim.ResetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
    }

    protected void HideInteractUI()
    {
        interactableAnim.ResetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

    }
}