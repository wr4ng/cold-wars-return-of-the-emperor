using System;
using UnityEngine;

[Serializable]
public class NetworkPrefab
{
    public EntityType type;
    public GameObject prefab;
}

public enum EntityType
{
    LocalPlayer,
    RemotePlayer,
    Bullet,
}