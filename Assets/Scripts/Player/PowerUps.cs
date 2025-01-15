using UnityEngine;

public class PowerUps : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        // Add powerup to player
        Debug.Log("Powerup picked up!");

        // Remove powerup from scene
        Destroy(gameObject);
    }
}