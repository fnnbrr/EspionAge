using System;
using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private Animator interactableAnim;
    private bool interactableOn = false;

    public delegate void OnInteractEventHandler(Interactable source);
    public event OnInteractEventHandler OnInteractEnd;


    // Start is called before the first frame update
    protected void Start()
    {
        interactableAnim = GetComponentInChildren<Animator>();
    }

 
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            if (!interactableOn)
            {
                ShowInteractUI();
            }
        }
    }


    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            HideInteractUI();
        }
    }


    protected void OnTriggerStay(Collider other)
    {
        // Show interactable button in the UI

        //
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            // User chooses to interact with the item
            if (Input.GetButtonDown("Interact"))
            {
                FaceInteractable(other.gameObject);
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
        Vector3 dirToFace = this.gameObject.transform.position + player.transform.position;
        player.transform.rotation = Quaternion.Euler(0f, dirToFace.y, 0f);
        // TODO: Animation of facing the interactable
    }


    ///////// WILL CHANGE ANIMATION LATER (FADE IN/OUT FOR NOW)
    private void ShowInteractUI()
    {
        interactableAnim.SetTrigger("FadeIn");
        interactableOn = true;
    }

    private void HideInteractUI()
    {
        interactableAnim.SetTrigger("FadeOut");
        interactableOn = false;
    }
}