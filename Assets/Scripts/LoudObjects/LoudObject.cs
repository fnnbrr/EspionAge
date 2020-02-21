using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoudObject : MonoBehaviour
{
    public float thrustForce;
    public Vector3 thrustDirection;
    public float shakeRadius;
    public float dropRadius;
    public float loudEffectRadius;
    private float distance;
    private Rigidbody rb;

    private Shader plateShader;
    public float minShake;
    public float maxShake;
    Renderer[] rends;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rends = GetComponents<Renderer> ();
        plateShader = Shader.Find("PlateShake");
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);

        if (distance <= dropRadius)
        {
            rb.AddForce(thrustDirection * thrustForce);
            NotifyChasingNurse();
        } 
        else if (distance <= shakeRadius)
        {
            float clampedDistance = Mathf.Clamp(distance, dropRadius, shakeRadius);
            float lerpedDistance = Mathf.Lerp(0f, 1f, (clampedDistance - dropRadius) / (shakeRadius - dropRadius));
            float dynamicShake = Mathf.Lerp(maxShake, minShake, lerpedDistance);
            print(dynamicShake);
            Shake(dynamicShake);
        } 
        else 
        {
            Shake(0f);
        }
    }

    void NotifyChasingNurse()
    {
        //TODO: Add a parameter to this function, layer mask
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, loudEffectRadius);
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
            if (rend.material.shader == plateShader){
                // SHAKE SHAKE SHAKE SHAKE
                rend.material.SetFloat("_ShakeAmplitude", shake);
            }
        }
    }
    
    void OnDrawGizmos()    
    {
        // Direction arrow of object falling
        Gizmos.color = Color.red;
        Vector3 directionToDraw = transform.TransformDirection(thrustDirection) * 5;
        Gizmos.DrawRay(transform.position, directionToDraw);

        // Radius sphere 
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, shakeRadius);

        // Loud object radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loudEffectRadius);
    }

}

