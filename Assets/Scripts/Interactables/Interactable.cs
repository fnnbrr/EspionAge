using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private Animator interactableAnim;
    private RectTransform interactTransform;
    private bool interactableOn = false;
    private GameObject player;

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
        }

        Vector3 dirToFace = transform.position - player.transform.position;
        dirToFace.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(dirToFace);

        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, rotation, 1);
        // TODO: Animation of facing the interactable


    }


    private void ShowInteractUI()
    {
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
    }

    private void HideInteractUI()
    {
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
    }
}