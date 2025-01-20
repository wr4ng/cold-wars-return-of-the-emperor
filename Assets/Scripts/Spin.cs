using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed = 8;

    private void Update()
    {
        transform.Rotate(0.6f * spinSpeed * Time.deltaTime, spinSpeed * Time.deltaTime, 0.3f * spinSpeed * Time.deltaTime);
    }
}
