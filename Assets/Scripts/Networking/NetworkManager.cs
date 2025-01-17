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

    private string host;
    private int port;

    private SpaceRepository repository;
    private string connectionString;
    private ISpace serverSpace;
    private Dictionary<Guid, ISpace> clientSpaces;

    private Guid myId;
    private ISpace mySpace;

    public bool IsServer { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;

    private Thread serverThread;
    private Thread clientThread;

    private int initialSeed;

    [SerializeField]
    private GameObject bulletPrefab;

    // NetworkTransform management
    [Header("Network Prefabs")]
    [SerializeField]
    private List<NetworkPrefab> networkPrefabs;
    private Dictionary<EntityType, GameObject> networkPrefabMap = new();
    private Dictionary<Guid, (EntityType, NetworkTransform)> networkTransforms = new();

    private ConcurrentQueue<Action> pendingActions = new();

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

    private void OnApplicationQuit()
    {
        Close();
    }

    public void StartServer(string host, int port)
    {
        this.host = host;
        this.port = port;

        //TODO Create type with method to create connectionString and gateString
        connectionString = string.Format("tcp://{0}:{1}?KEEP", host, port);

        // Setup server repository
        repository = new SpaceRepository();
        repository.AddGate(connectionString);

        serverSpace = new SequentialSpace();
        repository.AddSpace("server", serverSpace);

        // Setup local client repository
        //TODO: Setup local client as remote to share code
        myId = Guid.NewGuid();
        mySpace = new SequentialSpace();
        clientSpaces = new Dictionary<Guid, ISpace>() { { myId, mySpace } }; // Setup clientSpaces dictionary with initially only the local players space

        // Setup Maze
        initialSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        MazeGenerator.Instance.SetSeed(initialSeed);
        MazeGenerator.Instance.GenerateMaze();

        // Create local player for host
        Guid myPlayerID = Guid.NewGuid();
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
                // Close all gates
                repository.Dispose();
                if (serverThread.IsAlive)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }
                Debug.Log("Server closed");
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
        }
    }

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
                    case MessageType.UpdateNetworkTransform:
                        HandleUpdateNetworkTransform(message);
                        break;
                    case MessageType.SpawnBullet:
                        HandleSpawnBulletServer(message);
                        break;
                    default:
                        Debug.Log("unknown MessageType: " + message.Type);
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
                    case MessageType.SpawnBullet:
                        HandleSpawnBulletClient(message);
                        break;
                    default:
                        Debug.LogError("unknown MessageType: " + message.Type);
                        break;
                }

            }
        }
    }

    private void HandleJoin()
    {
        // Generate new ID for player
        Guid id = Guid.NewGuid();

        // Create new private space for player
        ISpace clientSpace = new SequentialSpace();
        clientSpaces[id] = clientSpace;
        repository.AddSpace(id.ToString(), clientSpace);

        // Send ID back to player
        Message m = new Message(MessageType.JoinResponse);
        m.WriteGuid(id);
        serverSpace.Put(0, MessageType.JoinResponse.ToString(), m.ToBytes());

        Debug.Log("New player joining with id: " + id.ToString());

        // Send all other NetworkTransforms to new player
        foreach (var (entityID, (entityType, networkTransform)) in networkTransforms)
        {
            //TODO: Fix this!
            Message m2 = new(MessageType.InstatiateNetworkTransform);
            m2.WriteGuid(entityID);
            EntityType type = (entityType == EntityType.LocalPlayer) ? EntityType.RemotePlayer : entityType;
            m2.WriteString(type.ToString());
            m2.WriteVector3(networkTransform.GetPosition());
            clientSpace.Put(m2.ToTuple());
        }

        // Create LocalPlayer at client and RemotePlayer at all others
        // Generate ID for this entity
        Guid newID = Guid.NewGuid();

        // Create messages
        Message newClientMessage = new(MessageType.InstatiateNetworkTransform);
        Message otherClientsMessage = new(MessageType.InstatiateNetworkTransform);

        newClientMessage.WriteGuid(newID);
        otherClientsMessage.WriteGuid(newID);

        newClientMessage.WriteString(EntityType.LocalPlayer.ToString());
        otherClientsMessage.WriteString(EntityType.RemotePlayer.ToString());

        Vector3 newPosition = MazeGenerator.Instance.GetRandomSpawnPoint();

        newClientMessage.WriteVector3(newPosition);
        otherClientsMessage.WriteVector3(newPosition);

        clientSpace.Put(newClientMessage.ToTuple());

        //TODO: Rename variables to be more clear
        foreach (var (cid, space) in clientSpaces)
        {
            if (id != cid)
            {
                space.Put(otherClientsMessage.ToTuple());
            }
        }

        // Send MazeInfo
        Message mazeInfo = new Message(MessageType.MazeInfo);
        mazeInfo.WriteInt(initialSeed);
        clientSpace.Put(mazeInfo.ToTuple());
    }

    private void HandleInstantiateNetworkTransform(Message message)
    {
        // Create a NetworkTransform at a given position and rotation with given ID.
        Guid entityID = message.ReadGuid();
        EntityType type = message.ReadEnum<EntityType>();
        Vector3 position = message.ReadVector3();

        // Instatiate object next frame
        pendingActions.Enqueue(() =>
        {
            // At to Dictionary of my (client) NetworkTransforms
            GameObject go = Instantiate(networkPrefabMap[type], position, Quaternion.identity);
            NetworkTransform networkTransform = go.GetComponent<NetworkTransform>();
            networkTransform.ID = entityID;
            networkTransforms.Add(entityID, (type, networkTransform));
        });
    }

    private void HandleSetNetworkTransform(Message message)
    {
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
                Debug.Log("ID of NetworkTransform not found");
            }
        });
    }

    private void HandleDestroyNetworkTransform(Message message)
    {
        Guid id = message.ReadGuid();

        bool found = networkTransforms.TryGetValue(id, out var value);
        if (found)
        {
            (_, NetworkTransform networkTransform) = value;
            networkTransforms.Remove(id);
            pendingActions.Enqueue(() =>
            {
                Destroy(networkTransform.gameObject);
            });
        }
        else
        {
            Debug.LogError($"Trying to destroy NetworkTransform with invalid id: {id}");
        }
    }

    //TODO: Could instead chunk together movement updates so they don't have to be specific packets
    private void HandleUpdateNetworkTransform(Message message)
    {
        // Broadcast movement update to clients
        //TODO: Create BroadcastMessage(Message m)
        //TODO: Maybe create a method to easily copy values in underlying byte-array
        Message broadcastMessage = new Message(MessageType.SetNetworkTransform);
        broadcastMessage.WriteGuid(message.ReadGuid());
        broadcastMessage.WriteVector3(message.ReadVector3());
        broadcastMessage.WriteQuarternion(message.ReadQuarternion());

        foreach (ISpace clientSpace in clientSpaces.Values)
        {
            clientSpace.Put(broadcastMessage.ToTuple());
        }
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

    private void HandleSpawnBulletServer(Message message)
    {
        Guid shooterID = message.ReadGuid();
        Vector3 bulletPositon = message.ReadVector3();
        Quaternion bulletQuarternion = message.ReadQuarternion();

        Message spawnBullet = new Message(MessageType.SpawnBullet);
        spawnBullet.WriteVector3(bulletPositon);
        spawnBullet.WriteQuarternion(bulletQuarternion);

        foreach ((_, ISpace s) in clientSpaces.Where(kvp => kvp.Key != shooterID))
        {
            s.Put(spawnBullet.ToTuple());
        }
    }

    private void HandleSpawnBulletClient(Message message)
    {
        Vector3 bulletPositon = message.ReadVector3();
        Quaternion bulletQuarternion = message.ReadQuarternion();

        pendingActions.Enqueue(() =>
        {
            Instantiate(bulletPrefab, bulletPositon, bulletQuarternion);
        });
    }

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
        Message message = new Message(MessageType.SpawnBullet);
        message.WriteGuid(myId);
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

    private void BroadcastMessage(Message message, Guid? excludeID = null)
    {
        IEnumerable<ISpace> receivers = clientSpaces.Where((pair) => pair.Key != excludeID).Select((pair) => pair.Value);
        foreach (ISpace clientSpace in receivers)
        {
            clientSpace.Put(message.ToTuple());
        }
    }
}
