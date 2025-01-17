using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2;
    public float speed = 10f;

    [SerializeField]
    private Rigidbody myRigidbody;
    [SerializeField]
    private NetworkTransform myNetworkTransform;

    void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        float moveDistance = speed * Time.fixedDeltaTime;
        // Cast ray forward to check if we hit something on the way. If we do, move forward and reflect at point
        bool hit = Physics.Raycast(new Ray(transform.position, transform.forward), out var hitInfo, moveDistance);
        if (hit)
        {
            if (NetworkManager.Instance.IsServer && hitInfo.collider.CompareTag("Player"))
            {

                NetworkTransform networkTransform = hitInfo.collider.gameObject.GetComponent<NetworkTransform>();
                NetworkManager.Instance.SendPlayerHit(networkTransform.ID);
                NetworkManager.Instance.SendDestroyNetworkTransform(myNetworkTransform.ID);
                Destroy(gameObject);
                return;
            }
            // Calculate how far the bullet wanted to move through the object, and apply move after reflection
            float extraMoveDistance = moveDistance - hitInfo.distance;
            Vector3 reflectedForward = Vector3.Reflect(transform.forward, hitInfo.normal);
            Vector3 newPositionn = hitInfo.point + reflectedForward * extraMoveDistance;

            myRigidbody.MovePosition(newPositionn);
            transform.forward = reflectedForward;
        }
        // Otherwise move forward
        else
        {
            Vector3 newPosition = transform.position + (transform.forward * speed * Time.deltaTime);
            myRigidbody.MovePosition(newPosition);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (NetworkManager.Instance.IsServer)
            {
                NetworkTransform networkTransform = collision.gameObject.GetComponent<NetworkTransform>();
                if (networkTransform == null) { return; }
                NetworkManager.Instance.SendPlayerHit(networkTransform.ID);
                NetworkManager.Instance.SendDestroyNetworkTransform(myNetworkTransform.ID);
                Destroy(gameObject);
            }
        }
    }

    // RemotePlayer has a trigger collider, so destroy bullet on hit
    private void OnTriggerEnter(Collider collider)
    {
        if (NetworkManager.Instance.IsServer)
        {
            NetworkTransform networkTransform = collider.gameObject.GetComponent<NetworkTransform>();
            NetworkManager.Instance.SendPlayerHit(networkTransform.ID);
            NetworkManager.Instance.SendDestroyNetworkTransform(myNetworkTransform.ID);
            Destroy(gameObject);
        }
    }

    private void OnGUI()
    {
        // Show ray to indicate where the bullet will move to next frame
        Debug.DrawLine(transform.position, transform.position + transform.forward * speed * Time.deltaTime, Color.red);
    }
}