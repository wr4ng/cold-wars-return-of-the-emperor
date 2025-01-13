using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using System.Runtime.CompilerServices;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2;

    Vector3 lastVelocity;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        lastVelocity = rb.linearVelocity;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("hit wall");
            var speed = lastVelocity.magnitude;
            var direction = Vector3.Reflect(lastVelocity.normalized, other.contacts[0].normal);
            rb.linearVelocity = direction * Mathf.Max(speed, 0f);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
