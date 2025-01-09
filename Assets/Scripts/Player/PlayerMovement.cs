using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;

    public float forwardForce = 2000f;
    public float rotationSpeed = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 velocity = Vector3.zero;

        if (Input.GetKey("w"))
        {
            velocity = transform.forward * forwardForce * Time.deltaTime;
        }

        rb.linearVelocity = velocity;

        if (Input.GetKey("q"))
        {
            transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey("e"))
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

    }
}
