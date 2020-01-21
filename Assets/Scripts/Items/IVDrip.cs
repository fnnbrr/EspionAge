using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IVDrip : MonoBehaviour
{
    private bool itemHeld;

    public float distanceFromCenter = 3.0f;
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

        SetHeldPosition(player);
        //Effect()
	}

 
    void DropItem()
	{
        itemHeld = false;
        lastInteractionTime = Time.time;
        gameObject.transform.parent = null;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    // While item is held, apply its effect (if any)
    void Effect()
	{

	}

    void SetHeldPosition(Transform player)
    {
        Vector3 rightSidePosition = new Vector3(player.position.x, gameObject.transform.position.y, player.position.z) + player.right * distanceFromCenter;
        Vector3 leftSidePosition = new Vector3(player.position.x, gameObject.transform.position.y, player.position.z) - player.right * distanceFromCenter;

        float rightSideDis = Vector3.Distance(rightSidePosition, gameObject.transform.position);
        float leftSideDis = Vector3.Distance(leftSidePosition, gameObject.transform.position);

        if (rightSideDis < leftSideDis)
        {
            gameObject.transform.position = rightSidePosition;
        }
        else
        {
            gameObject.transform.position = leftSidePosition;
        }
            
    }
}
