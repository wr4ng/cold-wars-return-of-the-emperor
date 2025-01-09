using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 2;
    public Rigidbody rigidbody;

    private void Update()
    {
        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {

    }
}
