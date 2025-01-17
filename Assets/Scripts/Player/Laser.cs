using UnityEngine;
using UnityEngine.InputSystem;

public class Laser : MonoBehaviour
{

    [SerializeField] private LineRenderer lineRenderer;
    private int numOfReflections = 5;
    public float range = 20f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer.positionCount = numOfReflections + 1;
    }

    public void EnableLineRenderer()
    {
        lineRenderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        CastRay(transform.position, transform.forward);
    }

    private void CastRay(Vector3 laserPos, Vector3 laserDir)
    {
        var currentLaserIndex = 0;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(currentLaserIndex, laserPos);

        for (var i = 0; i < numOfReflections; i++)
        {

            Ray ray = new Ray(laserPos, laserDir);

            if (Physics.Raycast(ray, out RaycastHit laserHit, range, ~0, QueryTriggerInteraction.Collide))
            {
                if (laserHit.collider.CompareTag("Wall"))
                {
                    shootLaser(laserHit);
                    currentLaserIndex++;
                    lineRenderer.positionCount = currentLaserIndex + 1;
                    lineRenderer.SetPosition(currentLaserIndex, laserHit.point);
                    laserDir = Vector3.Reflect(laserDir, laserHit.normal);
                    laserPos = laserHit.point;
                }
                else if (laserHit.collider.CompareTag("Player"))
                {
                    shootLaser(laserHit);
                    currentLaserIndex++;
                    lineRenderer.positionCount = currentLaserIndex + 1;
                    lineRenderer.SetPosition(currentLaserIndex, laserPos + laserDir * range);
                    break;
                }
                else
                {
                    shootLaser(laserHit);
                }
            }
            else
            {
                currentLaserIndex++;
                lineRenderer.positionCount = currentLaserIndex + 1;
                lineRenderer.SetPosition(currentLaserIndex, laserPos + laserDir * range);
                break;
            }
        }
    }

    public void shootLaser(RaycastHit laserHit)
    {
        PlayerPowerStats powerUp = GetComponent<PlayerPowerStats>();
        if (Input.GetKeyDown(KeyCode.F) && laserHit.collider.CompareTag("Player"))
        {
            Destroy(laserHit.collider.gameObject);
            powerUp.hasLaser = false;
            Debug.Log("Destroyed player");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            powerUp.hasLaser = false;
            Debug.Log("Lost Laser");

        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.CompareTag("Wall"))
    //     {
    //         CastRay(transform.position, transform.forward);
    //     }

    //     if (other.gameObject.CompareTag("Player"))
    //     {
    //         Destroy(other.gameObject);
    //     }
    // }
}
