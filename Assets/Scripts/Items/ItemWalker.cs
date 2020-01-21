using System.Collections;
using System.Collections.Generic;
using Application;
using UnityEngine;

public class ItemWalker : Item
{
    public float distanceFromCenter = 2.0f;


    // Sets the position of the item relative to the player
    public override void AcquireItem(Transform player)
    {
        base.AcquireItem(player);
        gameObject.GetComponent<BoxCollider>().isTrigger = false;
        SetHeldPosition(player);
        //Effect()
    }


    public override void DropItem()
    {
        base.DropItem();
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
    }

    // While item is held, apply its effect (if any)
    public new void Effect()
    {

    }

    public void SetHeldPosition(Transform player)
    {
        gameObject.transform.position = new Vector3(player.position.x, gameObject.transform.position.y, player.position.z) + player.forward * distanceFromCenter;

        gameObject.transform.rotation = player.rotation;

    }

}
