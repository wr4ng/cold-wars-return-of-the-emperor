using UnityEngine;
using UnityEngine.InputSystem;

public class Laser : MonoBehaviour
{

    [SerializeField] private LineRenderer lineRenderer;
    private int numOfReflections = 5;
    public float range = 20f;

    private PlayerPowerStats powerUp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer.positionCount = numOfReflections + 1;
        powerUp = GetComponent<PlayerPowerStats>();
    }

    public void EnableLineRenderer()
    {
        lineRenderer.enabled = true;
    }

    public void DisableLineRenderer()
    {
        if (lineRenderer != null) { lineRenderer.enabled = false; lineRenderer.positionCount = 0; Debug.Log("Line renderer disabled."); }

    }

    // Update is called once per frame
    void Update()
    {
        //if (!enabled || !lineRenderer.enabled)
        //{
        //    return;
        //}
        CastRay(transform.position, transform.forward);
        if (Input.GetKeyDown(KeyCode.F))
        {
            DisableLaser();
        }
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
                    currentLaserIndex++;
                    lineRenderer.positionCount = currentLaserIndex + 1;
                    lineRenderer.SetPosition(currentLaserIndex, laserHit.point);
                    laserDir = Vector3.Reflect(laserDir, laserHit.normal);
                    laserPos = laserHit.point;
                }
                else if (laserHit.collider.CompareTag("Player"))
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Destroy(laserHit.collider.gameObject);
                    }
                    currentLaserIndex++;
                    lineRenderer.positionCount = currentLaserIndex + 1;
                    lineRenderer.SetPosition(currentLaserIndex, laserPos + laserDir * range);
                    break;
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

    private void DisableLaser()
    {
        powerUp.hasLaser = false;
        DisableLineRenderer();
        enabled = false;
        Debug.Log("Laser and line renderer disabled.");
    }
}
