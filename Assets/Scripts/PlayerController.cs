using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float turnSpeed = 20f;
    public float canMoveRotationThreshold = 0.1f;

    private Rigidbody rb;
    private PlayerManager playerManager;

    private Vector3 movement;

    void Start()
    {
        rb = Utils.GetRequiredComponent<Rigidbody>(this);
        playerManager = Utils.GetRequiredComponent<PlayerManager>(this);
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movement.Set(horizontal, 0f, vertical);
        movement.Normalize();
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;    // to be used later for animations and such

        HandleControl(movement);
    }

    void HandleControl(Vector3 movementDirection)
    {
        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, movementDirection, turnSpeed * Time.fixedDeltaTime, 0f);

        HandleRotation(desiredForward);

        // Only start moving once we are close enough to our desired final rotation (which we are smoothly rotating towards)
        if ((movementDirection - desiredForward).magnitude < canMoveRotationThreshold)
        {
            HandleMovement();
        }
    }

    void HandleRotation(Vector3 desiredForward)
    {
        rb.MoveRotation(Quaternion.LookRotation(desiredForward));
    }

    void HandleMovement()
    {
        rb.MovePosition(rb.position + movement * movementSpeed * Time.fixedDeltaTime);

        //playerManager.HandleDecreaseStamina();
    }


}
