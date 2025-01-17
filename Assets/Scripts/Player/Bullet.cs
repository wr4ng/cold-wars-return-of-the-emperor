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
            //Destroy(gameObject);
        }
        //TODO: Maybe use layers?
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("hit wall");
        }

        transform.forward = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
        Vector3 newPosition = transform.position + (transform.forward * speed * Time.deltaTime);
        rb.MovePosition(newPosition);

        // Also move a bit forward
        Debug.Log(collision.gameObject);
    }
}