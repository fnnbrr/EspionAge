using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private Animator interactableAnim;
    private RectTransform interactTransform;
    private bool interactableOn = false;

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
            //TODO: Not sure if this works with 2 cameras?? Wait until later (using x negative scaling for now)
            interactTransform.LookAt(Camera.main.transform);
        }
    }


    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            if (!interactableOn)
            {
                ShowInteractUI();
            }
        }
    }


    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER) && interactableOn)
        {
            HideInteractUI();
        }
    }


    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            // User chooses to interact with the item
            if (Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
            {
                FaceInteractable(other.gameObject);

                //Collapse interact button text
                //HideInteractUI();
                OnInteract();
            }
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
    public void FaceInteractable(GameObject player)
    {
        Vector3 dirToFace = transform.position + player.transform.position;
        player.transform.rotation = Quaternion.Euler(0f, dirToFace.y, 0f);
        // TODO: Animation of facing the interactable
    }


    private void ShowInteractUI()
    {
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPIN);
        interactableOn = true;
    }

    private void HideInteractUI()
    {
        interactableAnim.SetTrigger(Constants.ANIMATION_INTERACTABLE_POPDOWN);
        interactableOn = false;
    }
}