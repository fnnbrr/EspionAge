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
        foreach (Rigidbody r in activeObjects)
        {
            r.AddForce(transform.forward * forceMagnitude);
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
