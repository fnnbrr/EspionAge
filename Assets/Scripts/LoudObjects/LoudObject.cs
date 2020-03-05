using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoudObject : MonoBehaviour
{
    [Header("Force Settings")]
    public float thrustForce;
    public float shakeRadius;
    public float dropRadius;
    public float despawnSec = 3.0f;
    private float distance;
    private Rigidbody rb;

    [Header("Awakeness Settings")]
    [Range(0f, StaminaBar.FILL_MAX)] public float awakenessIncreasePercentage = 0.2f;

    [Header("Fade Settings")]
    public bool fadeAndDestroyAfterThrow = true;
    public float destroyAfterSeconds = 3f;

    [Header("Shake Settings")]
    private Shader plateShader;
    public float minShake;
    public float maxShake;
    Renderer[] rends;

    private bool hasBeenBumped = false;
    private bool hasHit = false;
    private NoisePinger noisePing;

    public delegate void HasHitAction();
    public event HasHitAction OnHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rends = GetComponents<Renderer>();
        plateShader = Shader.Find(Constants.SHADER_NAME_SHAKE);
        noisePing = gameObject.GetComponent<NoisePinger>();
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);
        if (!hasBeenBumped && distance <= dropRadius)
        {
            Drop();
        } 
        else
        {
            float clampedDistance = Mathf.Clamp(distance, dropRadius, shakeRadius);
            float lerpedDistance = Mathf.Lerp(0f, 1f, (clampedDistance - dropRadius) / (shakeRadius - dropRadius));
            float dynamicShake = Mathf.Lerp(maxShake, minShake, lerpedDistance);
            Shake(dynamicShake);
        } 
    }

    public void Drop()
    {
        rb.AddForce(-GameManager.Instance.GetPlayerTransform().forward * thrustForce, ForceMode.Impulse);
        hasBeenBumped = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!hasBeenBumped || hasHit || other.gameObject.CompareTag("Player")) return;

        noisePing.SpawnNoisePing(other);
        hasHit = true;

        UIManager.Instance.staminaBar.InstantIncreaseAwarenessBy(awakenessIncreasePercentage);

        OnHit?.Invoke();

        if (fadeAndDestroyAfterThrow)
        {
            ObjectFader objectFader = GetComponent<ObjectFader>();
            if (objectFader)
            {
                objectFader.FadeToTransparent(destroyAfterSeconds, fadeAndDestroyAfterThrow);
            }
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
        // Shake radius sphere 
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, shakeRadius);
        
        // Drop radius sphere 
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dropRadius);
    }

}

