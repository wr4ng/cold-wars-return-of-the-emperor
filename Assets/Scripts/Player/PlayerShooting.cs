using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 2f;
    private float timer;

    [SerializeField]
    private InputActionReference shootAction;

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        if (shootAction.action.triggered && timer <= 0)
        {
            // Create bullet locally
            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            // Send request to shoot to server
            NetworkManager.Instance.SendSpawnBullet(bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            timer = 1 / fireRate;
        }
    }
}