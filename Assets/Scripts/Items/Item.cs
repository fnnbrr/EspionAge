using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class Item: MonoBehaviour, IItem
{
    protected bool itemHeld;

    public float minInteractionTime = 1.0f;
    protected float lastInteractionTime;

    // Start is called before the first frame update
    protected void Start()
    {
        itemHeld = false;
        lastInteractionTime = Time.time;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (Input.GetButtonDown("Item") && itemHeld && (Time.time - minInteractionTime > lastInteractionTime))
        {
            Debug.Log("Drop");
            DropItem();
        }

    }


    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PLAYER"))
        {
            // Show to the player that you can interact with the item


            // User chooses to interact with the item
            if (Input.GetButtonDown("Item") && !itemHeld && (Time.time - minInteractionTime > lastInteractionTime))
            {
                Debug.Log("Acquired");
                AcquireItem(other.gameObject.transform);
            }
        }
    }


    // Sets the position of the item relative to the player
    public virtual void AcquireItem(Transform player)
    {
        itemHeld = true;
        lastInteractionTime = Time.time;

        gameObject.transform.parent = player;
    }


    public virtual void DropItem()
    {
        itemHeld = false;
        lastInteractionTime = Time.time;
        gameObject.transform.parent = null;
    }

    public virtual void Effect()
    {
        throw new System.NotImplementedException();
    }
}
