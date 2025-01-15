using UnityEngine;

public class PowerUps : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp(other);
        }
    }

    void PickUp(Collider player)
    {
        // Add powerup to player
        Debug.Log("Powerup picked up!");

        // Add powerup to player depending on tag of powerup
        switch (gameObject.tag)
        {
            case "Laser":
                Debug.Log("Laser powerup picked up!");
                player.GetComponent<PlayerPowerStats>().hasLaser = true;
                break;

            case "Mini":
                Debug.Log("Mini powerup picked up!");
                player.GetComponent<PlayerPowerStats>().hasMini = true;
                break;

            default:
                Debug.Log("Powerup picked up!");
                break;
        }

        // Remove powerup from scene
        Destroy(gameObject);
    }
}