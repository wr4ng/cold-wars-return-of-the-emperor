using Unity.VisualScripting;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public float bulletSpeed, fireRate;
    public Transform bulletSpawnTransform;

    public GameObject bulletPrefab;

    private float timer;

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime / fireRate;
        }

        if (Input.GetKeyDown(KeyCode.Space) && timer <= 0)
        {
            Shoot();
        }

        // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hitInfo, 20f))
        // {
        //     Debug.Log("Blaster hit!");
        //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitInfo.distance, Color.green);
        // }
        // else
        // {
        //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 20f, Color.red);
        // }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawnTransform.forward * bulletSpeed, ForceMode.Impulse);

        timer = 1;

    }
}
