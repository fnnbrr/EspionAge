using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMinigame_CubeMovement : MonoBehaviour
{
    public float movementSpeed = 10f;

    private Rigidbody rb;

    void Start()
    {
         rb = Utils.GetRequiredComponent<Rigidbody>(this);
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");

        rb.MovePosition(rb.position + transform.forward * horizontal * movementSpeed * Time.fixedDeltaTime);
    }
}
