using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    private Animator interactableAnim;
    private bool interactableOn = false;

    // Start is called before the first frame update
    protected void Start()
    {
        interactableAnim = GetComponentInChildren<Animator>();
    }


    // Update is called once per frame
    void Update()
    {

    }

    // Show Interactable to player
    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            if (!interactableOn)
            {
                interactableAnim.SetTrigger("FadeIn");
                interactableOn = true;
            }
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            interactableAnim.SetTrigger("FadeOut");
            interactableOn = false;
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


    public virtual void OnInteract()
    {
        Debug.Log("Interacted");
    }


    // Changes the rotation of the player to face the interactable object
    public void FaceInteractable(GameObject player)
    {
        Vector3 dirToFace = this.gameObject.transform.position + player.transform.position;
        player.transform.rotation = Quaternion.Euler(0f, dirToFace.y, 0f);
    }

}
