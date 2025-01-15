using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{

    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 0.5f;
    private float timer;

    [SerializeField]
    private InputActionReference shootAction;

    // Update is called once per frame
    void Update()
    {

        PlayerPowerStats powerUp = GetComponent<PlayerPowerStats>();

        if (timer > 0)
        {
            timer -= Time.deltaTime / fireRate;
        }

        if (shootAction.action.triggered && timer <= 0)
        {
            switch (true)
            {
                case bool _ when powerUp.hasLaser:
                    Laser();
                    break;

                case bool _ when powerUp.hasMini:
                    Mini();
                    break;

                default:
                    Shoot();
                    break;
            }
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
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * bulletSpeed;

        timer = 1;
    }

    void Laser()
    {
        Debug.Log("Laser");
        timer = 1;
    }

    void Mini()
    {
        Debug.Log("Mini");
        timer = 1;
    }
}
