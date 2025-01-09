using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;

    public float forwardForce = 250f;
    public float rotationSpeed = 100f;

    public float backwardForce = -250f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            velocity = transform.forward * forwardForce * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            velocity = transform.forward * backwardForce * Time.deltaTime;

        }

        rb.linearVelocity = velocity;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

    }
}
