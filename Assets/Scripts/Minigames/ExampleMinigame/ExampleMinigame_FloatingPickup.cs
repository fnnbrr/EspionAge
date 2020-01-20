using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMinigame_FloatingPickup : MonoBehaviour
{
    public float rotateSpeed = 25f;
    public float bobbingSpeed = 0.2f;

    private Rigidbody rb;

    void Start()
    {
        rb = Utils.GetRequiredComponent<Rigidbody>(this);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + transform.up * Mathf.Sin(Time.time) * bobbingSpeed * Time.fixedDeltaTime);
        transform.Rotate(0f, rotateSpeed * Time.fixedDeltaTime, 0f);
    }
}
