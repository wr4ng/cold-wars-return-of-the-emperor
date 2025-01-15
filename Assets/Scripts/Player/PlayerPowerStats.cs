using UnityEngine;

public class PlayerPowerStats : MonoBehaviour
{
    // List of powerups the player can acquire
    public bool hasLaser = false;
    public bool hasMini = false;

    // This is used when a player has a powerup and picks up another one. If the player already has a power, the new one is not activated.
    public bool HasAnyPowerUp()
    {
        return hasLaser || hasMini;
    }
}
