using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoudObject : MonoBehaviour
{
    public float thrust;
    public Vector3 direction;
    public float shakeRadius;
    public float dropRadius;
    public float loudObjectRadius;
    private float distance;
    private Rigidbody rb;

    public float minShake;
    public float maxShake;
    Renderer[] rends;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rends = GetComponents<Renderer> ();
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);
        if (distance <= dropRadius)
        {
            rb.AddForce(direction * thrust);
            rb.useGravity = true;
            NotifyChasingNurse();
        } 
        else if (distance <= (dropRadius + 3))
        {
            Shake(maxShake);
        } 
        else if (distance <= shakeRadius)
        {
            float dynamicShake = Mathf.Lerp(maxShake, minShake, Mathf.Clamp(distance/5.0f, 0f, 1f));
            Shake(dynamicShake);
        } 
        else 
        {
            Shake(0f);
        }
    }

    void NotifyChasingNurse()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, loudObjectRadius);
        int i = 0;
        foreach (Collider collider in hitColliders)
        {
            // if collider is the chasing nurse 
            // {
            // call it's function 
            // }
        }
    }

    void Shake(float shake){
        foreach (Renderer rend in rends){
            if (rend.material.shader == Shader.Find("PlateShake")){
                // SHAKE SHAKE SHAKE SHAKE
                rend.material.SetFloat("_ShakeAmplitude", shake);
            }
        }
    }
    
    void OnDrawGizmos()    
    {
        // Direction arrow of object falling
        Gizmos.color = Color.red;
        Vector3 directionToDraw = transform.TransformDirection(direction) * 5;
        Gizmos.DrawRay(transform.position, directionToDraw);

        // Radius sphere 
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, shakeRadius);

        // Loud object radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loudObjectRadius);
    }

}

