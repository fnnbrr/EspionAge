using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class ItemIVDrip : Item
{

    public float distanceFromCenter = 3.0f;


    // Sets the position of the item relative to the player
    public override void AcquireItem(Transform player)
	{
        base.AcquireItem(player);
        gameObject.GetComponent<CapsuleCollider>().isTrigger = false;

        SetHeldPosition(player);
        //Effect()
	}

 
    public override void DropItem()
	{
        base.DropItem();
        gameObject.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    // While item is held, apply its effect (if any)
    public override void Effect()
	{

	}

    public void SetHeldPosition(Transform player)
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

    //TODO make health for IV DRIP
}
