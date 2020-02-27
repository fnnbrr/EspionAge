using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public GameObject brokenVersion;
    public float explosionForce = 1f;
    public float destroyInSeconds = 5f;

    private LoudObject loudObject;

    void Start()
    {
        loudObject = Utils.GetRequiredComponent<LoudObject>(this);
        loudObject.OnHit += BreakOnHit;
    }

    private void BreakOnHit()
    {
        GameObject instantiatedBrokenVersion = Instantiate(brokenVersion, transform.position, transform.rotation);

        ObjectFader objectFader = instantiatedBrokenVersion.GetComponent<ObjectFader>();
        if (objectFader)
        {
            objectFader.FadeToTransparent(secondsLater: destroyInSeconds, destroyAfter: true);
        }

        Rigidbody[] rigidbodies = instantiatedBrokenVersion.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rigidbodies)
        {
            r.AddExplosionForce(explosionForce, transform.position, 1f);
        }
        Destroy(gameObject);
    }
}
