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

    private void OnTriggerEnter(Collider other)
    {
        //TODO: Maybe use layers?
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("hit wall");
            //var speed = lastVelocity.magnitude;
            //var direction = Vector3.Reflect(lastVelocity.normalized, other.contacts[0].normal);
            //rb.linearVelocity = direction * Mathf.Max(speed, 0f);
        }

        if (other.gameObject.CompareTag("Player"))
        {
            //Destroy(other.gameObject);
            //Destroy(gameObject);
        }
    }
}