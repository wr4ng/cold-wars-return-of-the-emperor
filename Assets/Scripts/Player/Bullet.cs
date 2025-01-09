using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using System.Runtime.CompilerServices;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2;
    public Rigidbody rb;

    Vector3 lastVelocity;

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }

        lastVelocity = rb.linearVelocity;

    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("hit barrier");
        var speed = lastVelocity.magnitude;
        var direction = Vector3.Reflect(lastVelocity.normalized, other.contacts[0].normal);

        rb.linearVelocity = direction * Mathf.Max(speed, 0f);
    }
}
