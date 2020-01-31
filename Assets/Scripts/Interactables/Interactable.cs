using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private GameObject player;
    private Animator interactableAnim;
    private RectTransform interactTransform;

    private bool interactableOn = false;
    private bool playerRotating;
    

    public delegate void OnInteractEventHandler(Interactable source);
    public event OnInteractEventHandler OnInteractEnd;


    // Start is called before the first frame update
    protected void Start()
    {
        interactableAnim = GetComponentInChildren<Animator>();
        interactTransform = GetComponentInChildren<RectTransform>();
    }

    protected void Update()
    {
        //Ensures text is always facing the camera
        if(interactableOn)
        {
            // Interact Text always faces towards camera
            interactTransform.LookAt(CameraManager.Instance.GetActiveCameraTransform());

            // User chooses to interact with the item
            if (Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
            {
                FaceInteractable();

                OnInteract();
            }
     
        }
    }


    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            if (!interactableOn)
            {
                player = other.gameObject;
                interactableOn = true;

                ShowInteractUI();
            }
        }
    }


    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER) && interactableOn)
        {
            interactableOn = false;

            HideInteractUI();
        }
    }


    // Handle the dialogue for this interactable
    public virtual void OnInteract()
    {
        Debug.Log("Interacted");

        // Start Dialogue

        // Signal interact end after the dialogue is handled
        OnInteractEnd?.Invoke(this);
    }


    // Changes the rotation of the player to face the interactable object
    public void FaceInteractable()
    {
        // Ensures player game object has been assigned
        if (player is null)
        {
            Debug.LogError("Player refernce not found");
            return;
        }

        Vector3 dirToFace = transform.position - player.transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        // Animation of facing the interactable
        // Possible issue to prevent another coroutine being called if player is already rotating
        StartCoroutine(RotateAnimation(player, rotation, player.GetComponent<PlayerController>().turnSpeed));

    }

    // Coroutine that animates the rotation of the given object to the desiredRotation at a set turn speed
    // isRotating is used in the 
    private IEnumerator RotateAnimation(GameObject obj, Quaternion desiredRotation, float turnSpeed)
    {
        Quaternion startRotation = obj.transform.rotation;

        float t = 0;
    
        while (Mathf.Abs(Mathf.DeltaAngle(obj.transform.eulerAngles.y, desiredRotation.eulerAngles.y)) > 1.0f)
        {
            obj.transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, t);
            t += Time.deltaTime * turnSpeed;

            yield return null;
        }

    }


    private void ShowInteractUI()
    {
        interactableAnim.ResetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
    }

    private void HideInteractUI()
    {
        interactableAnim.ResetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
    }
}