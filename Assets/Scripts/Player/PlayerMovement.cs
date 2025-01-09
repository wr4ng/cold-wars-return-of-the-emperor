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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 input = movementAction.action.ReadValue<Vector2>();
        Vector3 velocity = Vector3.zero;

        if (input.y > 0)
        {
            velocity = transform.forward * forwardForce * Time.deltaTime;
        }
        else if (input.y < 0)
        {
            velocity = transform.forward * backwardForce * Time.deltaTime;
        }

        rb.linearVelocity = velocity;

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
