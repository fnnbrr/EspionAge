using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceZone : MonoBehaviour
{
    public float forceMagnitude;

    private List<Rigidbody> activeObjects;

    private void Start()
    {
        activeObjects = new List<Rigidbody>();
    }

    void Update()
    {
        List<Rigidbody> toRemove = new List<Rigidbody>();

        foreach (Rigidbody r in activeObjects)
        {
            // objects could be deleted unexpectedly
            if (r)
            {
                r.AddForce(transform.forward * forceMagnitude);
            }
            else
            {
                toRemove.Add(r);
            }
        }

        foreach (Rigidbody removing in toRemove)
        {
            activeObjects.Remove(removing);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody r = other.GetComponent<Rigidbody>();
        if (r)
        {
            activeObjects.Add(r);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody r = other.GetComponent<Rigidbody>();
        if (r)
        {
            activeObjects.Remove(r);
        }
    }
}
