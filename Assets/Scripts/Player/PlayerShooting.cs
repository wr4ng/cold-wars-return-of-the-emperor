using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;

public class PlayerShooting : MonoBehaviour
{

    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 0.5f;
    private float timer;
    public Laser laserscript;


    [SerializeField]
    private InputActionReference shootAction;

    private PlayerPowerStats powerUp;

    void Start()
    {
        powerUp = GetComponent<PlayerPowerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime / fireRate;
        }

        if (Input.GetKeyDown(KeyCode.L) || laserscript.enabled)
        {
            Laser();
        }
        else if (shootAction.action.triggered && timer <= 0)
        {
            Shoot();
            // switch (true)
            // {
            //     case bool _ when powerUp.hasLaser:
            //         Laser();
            //         break;

            //     case bool _ when powerUp.hasMini:
            //         Mini();
            //         break;

            //     default:
            //         Shoot();
            //         break;
            // }
        }
    }

    void Shoot()
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * bulletSpeed;

        timer = 1;
    }

    void Laser()
    {
        // timer = 1;
        powerUp.hasLaser = true;
        laserscript.enabled = true;
        laserscript.EnableLineRenderer();
        Debug.Log("Laser");
    }

    void Mini()
    {
        Debug.Log("Mini");
        timer = 1;
    }
}
