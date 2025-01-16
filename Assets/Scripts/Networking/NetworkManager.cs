using System;
using System.Threading;
using System.Net.Sockets;
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
    private Dictionary<Guid, ISpace> clientSpaces;
    private string host;
    private int port;

    private Guid myId;
    private ISpace mySpace;

    public bool IsServer { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;

    private Thread serverThread;
    private Thread clientThread;

    private int initialSeed;

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

    public void StartServer(string host, int port)
    {
        //TODO Create type with method to create connectionString and gateString
        this.host = host;
        this.port = port;
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

        // Create local player for host
        Guid myPlayerID = Guid.NewGuid();
        GameObject myPlayer = Instantiate(networkPrefabMap[EntityType.LocalPlayer], Vector3.zero, Quaternion.identity);
        NetworkTransform myNetworkTransform = myPlayer.GetComponent<NetworkTransform>();
        myNetworkTransform.ID = myPlayerID;
        networkTransforms.Add(myPlayerID, (EntityType.LocalPlayer, myNetworkTransform));

        // Setup maze info
        initialSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        MazeGenerator.Instance.SetSeed(initialSeed);
        MazeGenerator.Instance.GenerateMaze();

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
                repository.CloseGate(connectionString);
                if (serverThread.IsAlive)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }
                repository.CloseGate(connectionString);
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

    private void Update()
    {
        if (!IsRunning) { return; }

        //TODO: Debugging setup
        if (Input.GetKeyDown(KeyCode.Space))
        {
            try
            {
                Message m = new Message(MessageType.Hello);
                m.WriteInt(1234);
                m.WriteInt(-42);
                m.WriteString("yeet");

                serverSpace.Put(m.ToTuple());
                Debug.Log("[client] sent hello to server");
            }
            catch (SocketException e)
            {
                Debug.Log("[client] failed to send timestamp: " + e.ToString());
            }
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
                Debug.Log(message.Type);

                //TODO: Use Dictionary<MessageType, function(Message) -> void> to map MessageType to handler.
                switch (message.Type)
                {
                    case MessageType.Hello:
                        HandleHello(message);
                        break;
                    case MessageType.JoinRequest:
                        HandleJoin();
                        break;
                    case MessageType.UpdateNetworkTransform:
                        HandleUpdateNetworkTransform(message);
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
            //TODO: Hello isn't handled with the new system...
            IEnumerable<ITuple> tuples = mySpace.GetAll(Message.MessagePattern);
            foreach (ITuple tuple in tuples)
            {
                Message message = Message.FromTuple(tuple);
                Debug.Log(message.Type);

                //TODO: Use Dictionary<MessageType, function(Message) -> void> to map MessageType to handler.
                switch (message.Type)
                {
                    case MessageType.InstatiateNetworkTransform:
                        HandleInstantiateNetworkTransform(message);
                        break;
                    case MessageType.SetNetworkTransform:
                        HandleSetNetworkTransform(message);
                        break;
                    case MessageType.MazeInfo:
                        HandleMazeInfo(message);
                        break;
                    default:
                        Debug.Log("unknown MessageType: " + message.Type);
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

        newClientMessage.WriteVector3(Vector3.zero);
        otherClientsMessage.WriteVector3(Vector3.zero);

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

    //TODO: Remove once unused
    private void HandleHello(Message data)
    {

        Debug.Log("[hello] " + data.ReadInt() + ", " + data.ReadInt() + ", " + data.ReadString());
        foreach (ISpace clientSpace in clientSpaces.Values)
        {
            clientSpace.Put("someone said hello!");
        }
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
        try
        {
            NetworkTransform networkTransform = networkTransforms[id].Item2; //TODO: Named tuple?
            pendingActions.Enqueue(() =>
            {
                networkTransform.SetPosition(position);
            });
        }
        catch (Exception)
        {
            //TODO: Should be fixed once initial players are sent on join
            Debug.Log("ID not found");
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

    public void SendMovementUpdate(Guid id, Vector3 position)
    {
        Message message = new Message(MessageType.UpdateNetworkTransform);
        message.WriteGuid(id);
        message.WriteVector3(position);

        serverSpace.Put(message.ToTuple());
    }
}
