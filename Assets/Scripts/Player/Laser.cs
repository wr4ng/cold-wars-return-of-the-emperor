using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxReflections = 5;
    [SerializeField] private float range = 20f;

    private PlayerPowerStats powerUp;

    void Start()
    {
        lineRenderer.positionCount = maxReflections + 1;
        powerUp = GetComponent<PlayerPowerStats>();
    }

    public void EnableLineRenderer()
    {
        lineRenderer.enabled = true;
    }

    public void DisableLineRenderer()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
            Debug.Log("Line renderer disabled.");
        }
    }

    void Update()
    {
        CastRay(transform.position, transform.forward);

        if (Input.GetKeyDown(KeyCode.F))
        {
            DisableLaser();
        }
    }

    private void CastRay(Vector3 startPosition, Vector3 direction)
    {
        int currentPointIndex = 0;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(currentPointIndex, startPosition);

        for (int i = 0; i < maxReflections; i++)
        {
            Ray ray = new Ray(startPosition, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Collide))
            {
                currentPointIndex++;
                lineRenderer.positionCount = currentPointIndex + 1;
                lineRenderer.SetPosition(currentPointIndex, hit.point);

                if (hit.collider.CompareTag("Wall"))
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    startPosition = hit.point;
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Destroy(hit.collider.gameObject);
                    }

                    AddFinalLaserPoint(startPosition, direction);
                    break;
                }
            }
            else
            {
                AddFinalLaserPoint(startPosition, direction);
                break;
            }
        }
    }

    private void AddFinalLaserPoint(Vector3 position, Vector3 direction)
    {
        int finalPointIndex = lineRenderer.positionCount;
        lineRenderer.positionCount = finalPointIndex + 1;
        lineRenderer.SetPosition(finalPointIndex, position + direction * range);
    }

    private void DisableLaser()
    {
        powerUp.hasLaser = false;
        DisableLineRenderer();
        enabled = false;
        Debug.Log("Laser and line renderer disabled.");
    }
}
