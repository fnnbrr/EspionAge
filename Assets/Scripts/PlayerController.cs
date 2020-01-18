using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float turnSpeed = 20f;
    public float canMoveRotationThreshold = 0.1f;
    public float staminaDecrease = 0.01f;

    public StaminaBar staminaBar;

    private Rigidbody m_Rigidbody;

    private Vector3 m_Movement;
    private Quaternion m_Rotation;

    void Start()
    {
        m_Rigidbody = Utils.GetRequiredComponent<Rigidbody>(this);
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;    // to be used later for animations and such

        HandleControl(m_Movement);
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
        m_Rigidbody.MoveRotation(Quaternion.LookRotation(desiredForward));
    }

    void HandleMovement()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * movementSpeed * Time.fixedDeltaTime);

        StartCoroutine(staminaBar.DecreaseStaminaBy(staminaDecrease));
    }


}
