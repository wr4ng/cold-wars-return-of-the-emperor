using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public InputActionReference movementAction;

    public float forwardForce = 250f;
    public float backwardForce = -250f;
    public float rotationSpeed = 100f;
    public float deadZoneThreshold = 0.2f; // Define a dead zone threshold

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Reset angular velocity to stop spinning
        rb.angularVelocity = Vector3.zero;

        Vector2 input = movementAction.action.ReadValue<Vector2>();
        Vector3 velocity = Vector3.zero;

        // Apply dead zone threshold for forward/backward movement
        if (Mathf.Abs(input.y) > deadZoneThreshold)
        {
            if (input.y > 0)
            {
                velocity = transform.forward * forwardForce * Time.deltaTime;
            }
            else if (input.y < 0)
            {
                velocity = transform.forward * backwardForce * Time.deltaTime;
            }
        }

        rb.linearVelocity = velocity;

        // Apply dead zone threshold for rotation
        if (Mathf.Abs(input.x) > deadZoneThreshold)
        {
            if (input.x < 0)
            {
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
            }
            else if (input.x > 0)
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }
        }
    }
}
