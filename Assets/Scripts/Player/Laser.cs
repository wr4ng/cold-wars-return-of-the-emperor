using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float range = 20f;

    private PlayerPowerStats powerUp;

    void Start()
    {
        powerUp = GetComponent<PlayerPowerStats>();
        lineRenderer.enabled = false; // Initially disabled laser
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
        float remainingRange = range;
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(currentPointIndex, startPosition); // Sets the first point of the line renderer to the players position

        while (remainingRange > 0) // Casts the ray until it has no remaining range
        {
            Ray ray = new Ray(startPosition, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, remainingRange, ~0, QueryTriggerInteraction.Collide))
            {
                // Calculates the distance to hit and subtracts it from the remaining range
                float distanceToHit = Vector3.Distance(startPosition, hit.point);
                remainingRange -= distanceToHit;

                currentPointIndex++;
                lineRenderer.positionCount = currentPointIndex + 1;
                lineRenderer.SetPosition(currentPointIndex, hit.point); // Sets the next point of the line renderer to the hit point

                if (hit.collider.CompareTag("Wall"))
                {
                    direction = Vector3.Reflect(direction, hit.normal); // Reflects on walls using the Reflect() method and the normal vector of the hit point
                    startPosition = hit.point;
                }
                else if (hit.collider.CompareTag("Player")) // Checks for the player tag
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        Destroy(hit.collider.gameObject); // Destroys player if F is pressed
                    }
                    break; // If the ray hits a player, it stops
                }
            }
            else
            {
                AddFinalLaserPoint(startPosition, direction, remainingRange);
                break; // If the ray doesnt hit anything, it stops
            }
        }
    }

    private void AddFinalLaserPoint(Vector3 position, Vector3 direction, float remainingRange)
    {
        int finalPointIndex = lineRenderer.positionCount; // Gets the final point index
        lineRenderer.positionCount = finalPointIndex + 1;
        lineRenderer.SetPosition(finalPointIndex, position + direction * remainingRange); // Sets the final point of the line renderer to the remaining range
    }


    private void DisableLaser()
    {
        powerUp.hasLaser = false;
        DisableLineRenderer();
        enabled = false;
        Debug.Log("Laser and line renderer disabled.");
    }
}
