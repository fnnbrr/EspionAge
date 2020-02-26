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
    private float distance;
    private Rigidbody rb;

    private Shader plateShader;
    public float minShake;
    public float maxShake;
    Renderer[] rends;

    private bool hasBeenBumped = false;
    private bool hasHit = false;
    private NoisePing noisePing;

    public delegate void HasHitAction();
    public event HasHitAction OnHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rends = GetComponents<Renderer> ();
        plateShader = Shader.Find("PlateShake");
        noisePing = gameObject.GetComponent<NoisePing>();
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);

        if (!hasBeenBumped && distance <= dropRadius)
        {
            rb.AddForce(thrustDirection * thrustForce, ForceMode.Impulse);
            hasBeenBumped = true;
        } 
        else
        {
            float clampedDistance = Mathf.Clamp(distance, dropRadius, shakeRadius);
            float lerpedDistance = Mathf.Lerp(0f, 1f, (clampedDistance - dropRadius) / (shakeRadius - dropRadius));
            float dynamicShake = Mathf.Lerp(maxShake, minShake, lerpedDistance);
            Shake(dynamicShake);
        } 
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!hasBeenBumped || hasHit || other.gameObject.CompareTag("Player")) return;

        noisePing.SpawnNoisePing(other);
        hasHit = true;

        OnHit?.Invoke();
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

        // Shake radius sphere 
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, shakeRadius);
        
        // Drop radius sphere 
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }

}

