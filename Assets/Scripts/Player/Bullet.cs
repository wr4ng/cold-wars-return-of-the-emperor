using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2;
    public float speed = 10f;

    [SerializeField]
    private Rigidbody rb;

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        Vector3 newPosition = transform.position + (transform.forward * speed * Time.deltaTime);
        rb.MovePosition(newPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Destroy(other.gameObject);
            Destroy(gameObject);
        }

        transform.forward = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
        // Also move a bit forward to avoid re-colliding
        Vector3 newPosition = transform.position + (speed * Time.deltaTime * transform.forward);
        rb.MovePosition(newPosition);
    }

    // RemotePlayer has a trigger collider, so destroy bullet on hit
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}