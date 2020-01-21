using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVDrip : MonoBehaviour
{
    bool itemHeld;
    public float minInteractionTime = 1.0f;
    private float lastInteractionTime;

    // Start is called before the first frame update
    void Start()
    {
        itemHeld = false;
        lastInteractionTime = Time.time;    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Item") && itemHeld && (Time.time - minInteractionTime > lastInteractionTime))
        {
            Debug.Log("Drop");
            DropItem();
            gameObject.GetComponent<Collider>().enabled = true;
		}

    }


    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
		{
            // Show to the player that you can interact with the item

  
            // User chooses to interact with the item
            if (Input.GetButtonDown("Item") && !itemHeld && (Time.time - minInteractionTime > lastInteractionTime))
            {
                Debug.Log("Acquired");
                gameObject.GetComponent<Collider>().enabled = false;
                AcquireItem(other.gameObject.transform);
			}
		}
    }


    // Sets the position of the item relative to the player
    void AcquireItem(Transform player)
	{
        itemHeld = true;
        lastInteractionTime = Time.time;
        gameObject.transform.parent = player;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
	}

 
    void DropItem()
	{
        itemHeld = false;
        lastInteractionTime = Time.time;
        gameObject.transform.parent = null;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    void Effect()
	{

	}

    void SetHeldPosition()
    {

    }
}
