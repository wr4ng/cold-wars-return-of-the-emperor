using System;
using TreeEditor;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{
    public Guid ID;
    public EntityType Type;
    public bool IsOwner;

    [SerializeField]
    private int updateRate = 30;
    private float timer = 0;

    private Vector3 lastPosition;

    private void Update()
    {
        lastPosition = transform.position;
        // Only update position if current client is owner of NetworkTransform and we're connected
        if (!NetworkManager.Instance.IsRunning || !IsOwner)
        {
            return;
        }
        if (timer >= 1 / updateRate)
        {
            NetworkManager.Instance.SendMovementUpdate(ID, transform.position);
            timer = 0;
        }
        timer += Time.deltaTime;
    }

    public void SetPosition(Vector3 position)
    {
        //TODO: Handle reconciliation when IsOwner and positions don't match
        if (IsOwner)
        {
            return;
        }
        transform.position = position;
    }

    public Vector3 GetPosition() => lastPosition;

    //TODO: Handle destroying NetworkTransforms
    //public void Destroy()
    //{
    //    NetworkManager.Instance.DestroyNetworkTransform();
    //}
}