using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoudObject : MonoBehaviour
{
    public float thrust = 1.0f;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            print(other.transform);
            print("UR IN BRO");
            // wobble wobble trigger alex's shader
            // make object fall 
            rb.AddForce(transform.forward * 180);
            rb.useGravity = true;
        }

    }
}
