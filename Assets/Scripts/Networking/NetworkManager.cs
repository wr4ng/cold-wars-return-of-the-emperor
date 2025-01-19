using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private SpaceRepository repository;
    private string connectionString;
    private ISpace serverSpace;
    private Dictionary<Guid, Client> clients;

    private Guid myId;
    private ISpace mySpace;

    public bool IsServer { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;

    private Thread serverThread;
    private Thread clientThread;

    private int mazeSeed;
    private int alivePlayers;

    // NetworkTransform management
    [Header("Network Prefabs")]
    [SerializeField]
    private List<NetworkPrefab> networkPrefabs;
    private Dictionary<EntityType, GameObject> networkPrefabMap = new();
    private Dictionary<Guid, (EntityType type, NetworkTransform networkTransform)> networkTransforms = new();

    private ConcurrentQueue<Action> pendingActions = new();

    #region Unity ovverrides
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple NetworkManagers!");
        }

        // Setup network prefabs as Dictionary instead of list
        networkPrefabMap = new();
        foreach (NetworkPrefab networkPrefab in networkPrefabs)
        {
            networkPrefabMap.Add(networkPrefab.type, networkPrefab.prefab);
        }
    }

    private void FixedUpdate()
    {
        if (IsRunning)
        {
            while (!pendingActions.IsEmpty)
            {
                bool success = pendingActions.TryDequeue(out Action action);
                if (!success)
                {
                    Debug.LogError("failed to dequeue pending action");
                    return;
                }
                action();
            }
            // After all queued actions have been perfomed, send updated NetworkTransform info of objects that has moved
            foreach (var (id, (_, networkTransform)) in networkTransforms)
            {
                if (networkTransform.hasMoved)
                {
                    Message message = new Message(MessageType.SetNetworkTransform);
                    message.WriteGuid(id);
                    message.WriteVector3(networkTransform.transform.position);
                    message.WriteQuarternion(networkTransform.transform.rotation);
                    BroadcastMessage(message);
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        Close();
    }
    #endregion

    #region Network setup/close
    public void StartServer(string host, int port)
    {
        connectionString = string.Format("tcp://{0}:{1}?KEEP", host, port);

        // Setup server repository
        repository = new SpaceRepository();
        repository.AddGate(connectionString);

        serverSpace = new SequentialSpace();
        repository.AddSpace("server", serverSpace);

        // Setup local client repository
        myId = Guid.NewGuid();
        Guid myPlayerID = Guid.NewGuid();
        mySpace = new SequentialSpace();
        // Setup clients dictionary with initially only the local player
        clients = new Dictionary<Guid, Client>() { { myId, new(mySpace, myPlayerID) } };

        // Setup Maze
        mazeSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        MazeGenerator.Instance.SetSeed(mazeSeed);
        MazeGenerator.Instance.GenerateMaze();
        alivePlayers = 1;

        // Create local player for host
        GameObject myPlayer = Instantiate(networkPrefabMap[EntityType.LocalPlayer], MazeGenerator.Instance.GetRandomSpawnPoint(), Quaternion.identity);
        NetworkTransform myNetworkTransform = myPlayer.GetComponent<NetworkTransform>();
        myNetworkTransform.ID = myPlayerID;
        networkTransforms.Add(myPlayerID, (EntityType.LocalPlayer, myNetworkTransform));

        // Start server thread
        serverThread = new Thread(() => RunServerListen());
        serverThread.Start();

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        IsServer = true;
        IsRunning = true;
        Debug.Log("Server started with connection string: " + connectionString);
    }

    public void StartClient(string host, int port)
    {
        connectionString = string.Format("tcp://{0}:{1}/server?KEEP", host, port);
        Debug.Log("Trying to connect to: " + connectionString + "...");
        serverSpace = new RemoteSpace(connectionString);

        // Send JoinRequest
        Message joinRequest = new(MessageType.JoinRequest);
        serverSpace.Put(joinRequest.ToTuple());
        Debug.Log("Connected to: " + connectionString);

        // Wait for JoinResponse
        //TODO: Fix different for pattern when client need to read from serverspace
        ITuple responseTuple = serverSpace.Get(0, MessageType.JoinResponse.ToString(), typeof(byte[]));
        Message joinResponse = Message.FromBytes((byte[])responseTuple[2]);
        myId = joinResponse.ReadGuid();
        //TODO: Receive info on number of existing players

        Debug.Log("Received id: " + myId.ToString());

        // Connect to private space
        string privateConnectionString = string.Format("tcp://{0}:{1}/{2}?KEEP", host, port, myId.ToString());
        Debug.Log("Trying to connect to: " + privateConnectionString + "...");
        mySpace = new RemoteSpace(privateConnectionString);
        mySpace.Put("ping"); // TODO: Determine if it's needed to send data to space to test connection
        Debug.Log("Connected to: " + privateConnectionString);

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        IsServer = false;
        IsRunning = true;
    }

    public void Close()
    {
        if (IsRunning)
        {
            IsRunning = false;
            if (IsServer)
            {
                Debug.Log("Closing server...");
                // Send Disconnect to all clients
                Message disconnect = new(MessageType.Disconnect);
                BroadcastMessage(disconnect);
                // Close all gates
                repository.Dispose();
                if (serverThread.IsAlive)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }
                Debug.Log("Server closed");
            }
            else
            {
                // Tell server that we disconnect
                Message disconnect = new(MessageType.Disconnect);
                disconnect.WriteGuid(myId);
                serverSpace.Put(disconnect.ToTuple());
            }
            Debug.Log("Closing client...");
            if (clientThread.IsAlive)
            {
                clientThread.Abort();
                clientThread.Join();
            }
            Debug.Log("Client closed");
        }
    }
    #endregion

    #region Thread listeners
    private void RunServerListen()
    {
        Debug.Log("[server] server listen thread started...");
        while (IsRunning)
        {
            IEnumerable<ITuple> tuples = serverSpace.GetAll(Message.MessagePattern);

            foreach (ITuple t in tuples)
            {
                Message message = Message.FromTuple(t);

                //TODO: Use Dictionary<MessageType, function(Message) -> void> to map MessageType to handler.
                switch (message.Type)
                {
                    case MessageType.JoinRequest:
                        HandleJoin();
                        break;
                    case MessageType.Disconnect:
                        HandleDisconnectServer(message);
                        break;
                    case MessageType.UpdateNetworkTransform:
                        HandleUpdateNetworkTransform(message);
                        break;
                    case MessageType.SpawnBullet:
                        HandleSpawnBulletServer(message);
                        break;
                    default:
                        Debug.Log("[server] unknown MessageType: " + message.Type);
                        break;
                }
            }
        }
    }

    private void RunClientListen()
    {
        Debug.Log("[client] client listen thread started...");
        while (IsRunning)
        {
            IEnumerable<ITuple> tuples = mySpace.GetAll(Message.MessagePattern);
            foreach (ITuple tuple in tuples)
            {
                Message message = Message.FromTuple(tuple);

                //TODO: Use Dictionary<MessageType, function(Message) -> void> to map MessageType to handler.
                switch (message.Type)
                {
                    case MessageType.Disconnect:
                        HandleDisconnectClient();
                        break;
                    case MessageType.InstatiateNetworkTransform:
                        HandleInstantiateNetworkTransform(message);
                        break;
                    case MessageType.SetNetworkTransform:
                        HandleSetNetworkTransform(message);
                        break;
                    case MessageType.DestroyNetworkTransform:
                        HandleDestroyNetworkTransform(message);
                        break;
                    case MessageType.MazeInfo:
                        HandleMazeInfo(message);
                        break;
                    case MessageType.PlayerHit:
                        HandlePlayerHit(message);
                        break;
                    case MessageType.NewRound:
                        HandleNewRound(message);
                        break;
                    default:
                        Debug.LogError("[client] unknown MessageType: " + message.Type);
                        break;
                }

            }
        }
    }
    #endregion

    #region Network helpers
    private void BroadcastMessage(Message message, Guid? excludeID = null)
    {
        IEnumerable<ISpace> receivers = clients.Where((pair) => pair.Key != excludeID).Select((pair) => pair.Value.space);
        foreach (ISpace clientSpace in receivers)
        {
            clientSpace.Put(message.ToTuple());
        }
    }
    #endregion

    #region Server handlers
    private void HandleJoin()
    {
        // Generate new ID for client 
        Guid clientID = Guid.NewGuid();
        // Generate ID for clients player object
        Guid playerID = Guid.NewGuid();

        // Create new private space for player
        ISpace clientSpace = new SequentialSpace();
        clients[clientID] = new(clientSpace, playerID);
        repository.AddSpace(clientID.ToString(), clientSpace);

        // Send ID back to player
        Message m = new Message(MessageType.JoinResponse);
        m.WriteGuid(clientID);
        serverSpace.Put(0, MessageType.JoinResponse.ToString(), m.ToBytes());

        Debug.Log("[server] new player joining with id: " + clientID.ToString());

        // Send all other existing NetworkTransforms to new player
        foreach (var (entityID, (entityType, networkTransform)) in networkTransforms)
        {
            Message networkTransformMessage = new(MessageType.InstatiateNetworkTransform);
            networkTransformMessage.WriteGuid(entityID);
            // Make sure hosts local player is created a remote player on new client
            EntityType type = (entityType == EntityType.LocalPlayer) ? EntityType.RemotePlayer : entityType;
            networkTransformMessage.WriteEnum(type);
            networkTransformMessage.WriteVector3(networkTransform.GetPosition());
            networkTransformMessage.WriteQuarternion(networkTransform.GetRotation());
            clientSpace.Put(networkTransformMessage.ToTuple());
        }

        // Create LocalPlayer at client and RemotePlayer at all others
        Vector3 newPosition = MazeGenerator.Instance.GetRandomSpawnPoint();

        // Send message to new client to create local player
        Message newClientMessage = new(MessageType.InstatiateNetworkTransform);
        newClientMessage.WriteGuid(playerID);
        newClientMessage.WriteString(EntityType.LocalPlayer.ToString());
        newClientMessage.WriteVector3(newPosition);
        newClientMessage.WriteQuarternion(Quaternion.identity);
        clientSpace.Put(newClientMessage.ToTuple());

        // Send message to all other clients to create a remote player
        Message otherClientsMessage = new(MessageType.InstatiateNetworkTransform);
        otherClientsMessage.WriteGuid(playerID);
        otherClientsMessage.WriteString(EntityType.RemotePlayer.ToString());
        otherClientsMessage.WriteVector3(newPosition);
        otherClientsMessage.WriteQuarternion(Quaternion.identity);
        BroadcastMessage(otherClientsMessage, excludeID: clientID);

        // Send MazeInfo
        Message mazeInfo = new Message(MessageType.MazeInfo);
        mazeInfo.WriteInt(mazeSeed);
        clientSpace.Put(mazeInfo.ToTuple());
        alivePlayers += 1;
    }

    private void HandleDisconnectServer(Message message)
    {
        Guid id = message.ReadGuid();

        // Read out and remove client
        Client c = clients[id];
        clients.Remove(id);

        //TODO: Handle if player is alive while disconnecting

        // Destroy clients player object on remaining clients
        Message destroyMessage = new(MessageType.DestroyNetworkTransform);
        destroyMessage.WriteGuid(c.transformID);
        BroadcastMessage(destroyMessage);
    }

    private void HandleUpdateNetworkTransform(Message message)
    {
        Guid id = message.ReadGuid();
        Vector3 position = message.ReadVector3();
        Quaternion rotation = message.ReadQuarternion();

        // Set position of NetworkTransform on server
        pendingActions.Enqueue(() =>
        {
            bool found = networkTransforms.TryGetValue(id, out var value);
            if (found)
            {
                (_, NetworkTransform networkTransform) = value;
                networkTransform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                Debug.Log($"[server] trying to set position of NetworkTransform with unknown id: {id}");
            }
        });
    }

    private void HandleSpawnBulletServer(Message message)
    {
        Guid shooterID = message.ReadGuid();
        Guid bulletID = message.ReadGuid();
        Vector3 bulletPositon = message.ReadVector3();
        Quaternion bulletQuarternion = message.ReadQuarternion();

        Message spawnBullet = new(MessageType.InstatiateNetworkTransform);
        spawnBullet.WriteGuid(bulletID);
        spawnBullet.WriteEnum(EntityType.Bullet);
        spawnBullet.WriteVector3(bulletPositon);
        spawnBullet.WriteQuarternion(bulletQuarternion);

        BroadcastMessage(spawnBullet, excludeID: shooterID);
    }
    #endregion

    #region Client handlers
    private void HandleInstantiateNetworkTransform(Message message)
    {
        // Create a NetworkTransform at a given position and rotation with given ID.
        Guid entityID = message.ReadGuid();
        EntityType type = message.ReadEnum<EntityType>();
        Vector3 position = message.ReadVector3();
        Quaternion rotation = message.ReadQuarternion();

        // Instatiate object next frame
        pendingActions.Enqueue(() =>
        {
            // Add to Dictionary of my (client) NetworkTransforms
            GameObject go = Instantiate(networkPrefabMap[type], position, rotation);
            NetworkTransform networkTransform = go.GetComponent<NetworkTransform>();
            networkTransform.ID = entityID;
            networkTransforms.Add(entityID, (type, networkTransform));
        });
    }

    private void HandleSetNetworkTransform(Message message)
    {
        // Server sets position immediately upon receiving message
        if (IsServer) return;

        // Set position and rotation of NetworkTransform with given ID
        Guid id = message.ReadGuid();
        Vector3 position = message.ReadVector3();
        Quaternion rotation = message.ReadQuarternion();

        pendingActions.Enqueue(() =>
        {
            if (networkTransforms.TryGetValue(id, out var value))
            {
                (_, NetworkTransform networkTransform) = value;
                networkTransform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                Debug.Log("[client] ID of NetworkTransform not found");
            }
        });
    }

    private void HandleDestroyNetworkTransform(Message message)
    {
        Guid id = message.ReadGuid();

        pendingActions.Enqueue(() =>
        {
            bool found = networkTransforms.TryGetValue(id, out var value);
            if (found)
            {
                (_, NetworkTransform networkTransform) = value;
                networkTransforms.Remove(id);
                Destroy(networkTransform.gameObject);
            }
            else
            {
                Debug.Log($"[client] trying to destroy NetworkTransform with invalid id: {id}");
            }
        });
    }

    private void HandleMazeInfo(Message message)
    {
        int seed = message.ReadInt();
        pendingActions.Enqueue(() =>
        {
            MazeGenerator.Instance.SetSeed(seed);
            MazeGenerator.Instance.GenerateMaze();
        });
    }

    private void HandleDisconnectClient()
    {
        pendingActions.Enqueue(() =>
        {
            MazeGenerator.Instance.ClearMaze();
            Close();
        });
    }

    private void HandlePlayerHit(Message message)
    {
        Guid transformID = message.ReadGuid();
        // Disable the corresponding GameObject
        pendingActions.Enqueue(() =>
        {
            if (networkTransforms.TryGetValue(transformID, out var value))
            {
                value.networkTransform.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("[client] can't handle PlayerHit as transformID is not present in networkTransforms");
            }
        });
    }

    private void HandleNewRound(Message message)
    {
        int seed = message.ReadInt();
        Guid id = message.ReadGuid();
        Vector3 newPosition = message.ReadVector3();

        pendingActions.Enqueue(() =>
        {
            // Setup new maze
            MazeGenerator.Instance.ClearMaze();
            MazeGenerator.Instance.SetSeed(seed);
            MazeGenerator.Instance.GenerateMaze();

            // Re-enable all NetworkTransforms
            foreach (var (_, (_, networkTransform)) in networkTransforms)
            {
                if (networkTransform != null)
                {
                    networkTransform.gameObject.SetActive(true);
                }
            }

            // Set my new position
            networkTransforms[id].networkTransform.transform.position = newPosition;
        });
    }

    #endregion

    #region Message senders
    public void SendMovementUpdate(Guid id, Vector3 position, Quaternion rotation)
    {
        Message message = new Message(MessageType.UpdateNetworkTransform);
        message.WriteGuid(id);
        message.WriteVector3(position);
        message.WriteQuarternion(rotation);

        serverSpace.Put(message.ToTuple());
    }

    public void SendSpawnBullet(Vector3 bulletPosition, Quaternion bulletRotation)
    {
        Guid bulletID = Guid.NewGuid();

        // Spawn local bullet on client immediately
        GameObject bullet = Instantiate(networkPrefabMap[EntityType.Bullet], bulletPosition, bulletRotation);
        NetworkTransform networkTransform = bullet.GetComponent<NetworkTransform>();
        networkTransform.ID = bulletID;
        networkTransforms.Add(bulletID, (EntityType.Bullet, networkTransform));

        Message message = new(MessageType.SpawnBullet);
        message.WriteGuid(myId);
        message.WriteGuid(bulletID);
        message.WriteVector3(bulletPosition);
        message.WriteQuarternion(bulletRotation);

        serverSpace.Put(message.ToTuple());
    }

    public void SendDestroyNetworkTransform(Guid networkTransformID)
    {
        networkTransforms.Remove(networkTransformID);
        Message m = new(MessageType.DestroyNetworkTransform);
        m.WriteGuid(networkTransformID);
        BroadcastMessage(m, excludeID: myId);
    }

    public void SendPlayerHit(Guid transformID)
    {
        alivePlayers -= 1;
        if (alivePlayers <= 1)
        {
            //TODO: Maybe start timer to not reset game instantly
            NewRound();
            return;
        }

        // Disable player locally on Server
        networkTransforms[transformID].networkTransform.gameObject.SetActive(false);

        // Send message to clients to disable the hit player
        Message message = new(MessageType.PlayerHit);
        message.WriteGuid(transformID);
        BroadcastMessage(message);
    }
    #endregion

    private void NewRound()
    {
        // Reset maze
        mazeSeed += 1;
        MazeGenerator.Instance.ClearMaze();
        MazeGenerator.Instance.SetSeed(mazeSeed);
        MazeGenerator.Instance.GenerateMaze();

        // Re-enable all NetworkTransforms
        //TODO: Destroy bullets so they don't carry over
        foreach (var (_, (_, networkTransform)) in networkTransforms)
        {
            if (networkTransform != null)
            {
                networkTransform.gameObject.SetActive(true);
            }
        }

        alivePlayers = clients.Count;

        // Send each player NewRound message
        foreach (var (id, c) in clients)
        {
            if (id == myId)
            {
                networkTransforms[c.transformID].networkTransform.transform.position = MazeGenerator.Instance.GetRandomSpawnPoint();
                continue;
            }
            Message message = new(MessageType.NewRound);
            message.WriteInt(mazeSeed);
            message.WriteGuid(c.transformID);
            message.WriteVector3(MazeGenerator.Instance.GetRandomSpawnPoint());
            c.space.Put(message.ToTuple());
        }
    }
}
