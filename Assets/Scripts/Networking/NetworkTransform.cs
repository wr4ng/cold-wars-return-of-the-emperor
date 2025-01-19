using System;
using UnityEngine;

public class NetworkTransform : MonoBehaviour
{
    public Guid ID;
    public EntityType Type;
    public bool IsOwner;
    public bool SyncPosition;

    [SerializeField]
    private int updateRate = 30;
    private float timer = 0;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    // Used to check if this objects position have been updated this frame, and should be transmitted to other clients (server perspective)
    public bool hasMoved;

    private void Update()
    {
        if (!SyncPosition) { return; }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
        // Only update position if current client is owner of NetworkTransform and we're connected
        if (!NetworkManager.Instance.IsRunning || !IsOwner)
        {
            return;
        }
        if (timer >= 1 / updateRate)
        {
            //TODO: Maybe only send movement if object has move more than some fixed distance since last position
            NetworkManager.Instance.SendMovementUpdate(ID, transform.position, transform.rotation);
            timer = 0;
        }
        timer += Time.deltaTime;
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Instance.IsServer)
        {
            hasMoved = true;
        }
        //TODO: Handle reconciliation when IsOwner and positions don't match
        if (IsOwner)
        {
            return;
        }
        transform.SetPositionAndRotation(position, rotation);
    }

    public Vector3 GetPosition() => lastPosition;
    public Quaternion GetRotation() => lastRotation;

    //TODO: Handle destroying NetworkTransforms
    //public void Destroy()
    //{
    //    NetworkManager.Instance.DestroyNetworkTransform();
    //}
}