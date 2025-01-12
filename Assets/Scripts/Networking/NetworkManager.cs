using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;

using UnityEngine;
using System;
using System.Net;
using System.Collections.Concurrent;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [SerializeField] public GameObject remotePlayerPrefab;

    private SpaceRepository repository;
    private string connectionString;
    private ISpace serverSpace;
    private Dictionary<Guid, ISpace> clientSpaces;
    private string host;
    private int port;

    private Guid myId;
    private ISpace mySpace;

    public bool isServer { get; private set; } = false;
    public bool isRunning { get; private set; } = false;

    private Thread serverThread;
    private Thread clientThread;

    private ConcurrentQueue<Action> pendingActions;

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

        pendingActions = new ConcurrentQueue<Action>();

        // Start server thread
        serverThread = new Thread(() => RunServerListen());
        serverThread.Start();

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        isServer = true;
        isRunning = true;
        Debug.Log("Server started with connection string: " + connectionString);
    }

    public void StartClient(string host, int port)
    {
        connectionString = string.Format("tcp://{0}:{1}/server?KEEP", host, port);
        Debug.Log("Trying to connect to: " + connectionString + "...");
        serverSpace = new RemoteSpace(connectionString);
        serverSpace.Put("join");
        Debug.Log("Connected to: " + connectionString);

        ITuple tuple = serverSpace.Get("id", typeof(string));
        myId = new Guid((string)tuple[1]);

        Debug.Log("Received id: " + myId.ToString());

        //TODO: Receive info on number of existing players

        // Connect to private space
        string privateConnectionString = string.Format("tcp://{0}:{1}/{2}?KEEP", host, port, myId.ToString());
        Debug.Log("Trying to connect to: " + privateConnectionString + "...");
        mySpace = new RemoteSpace(privateConnectionString);
        mySpace.Put("ping"); // TODO: Determine if it's needed to send data to space to test connection
        Debug.Log("Connected to: " + privateConnectionString);

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        isServer = false;
        isRunning = true;
    }

    public void Close()
    {
        if (isRunning)
        {
            isRunning = false;
            if (isServer)
            {
                Debug.Log("Closing server...");
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
        if (!isRunning)
        {
            return;
        }
        //TODO: Debugging setup. Sending Message object with 2 ints written in a tuple on the form ("hello", byte[])
        if (Input.GetKeyDown(KeyCode.S))
        {
            try
            {
                Message m = new Message();
                m.WriteInt(1234);
                m.WriteInt(-42);

                serverSpace.Put("hello", m.ToArray());
                Debug.Log("[client] sent hello to server");
            }
            catch (SocketException e)
            {
                Debug.Log("[client] failed to send timestamp: " + e.ToString());
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            try
            {
                ITuple t = serverSpace.GetP(typeof(string), typeof(byte[]));
                if (t == null)
                {
                    Debug.Log("[client] Nothing there...");
                    return;
                }
                Message m = new Message((byte[])t[1]);
                Debug.Log("[client] " + t[0] + ", " + m.ReadInt() + ", " + m.ReadInt());
            }
            catch (SocketException e)
            {

                Debug.Log("[client] failed to send timestamp: " + e.ToString());
            }
        }
    }

    private void FixedUpdate()
    {
        if (isServer && isRunning)
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
        while (isRunning)
        {
            IEnumerable<ITuple> tuples = serverSpace.GetAll(typeof(string));

            foreach (ITuple t in tuples)
            {
                string message = (string)t[0];
                Debug.Log(message);
                switch (message)
                {
                    case "join":
                        HandleJoin();
                        break;
                    case "hello":
                        HandleHello();
                        break;
                    default:
                        Debug.Log("unknown message: " + message);
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
        serverSpace.Put("id", id.ToString());

        Debug.Log("New player joining with id: " + id.ToString());

        // Create new player on local client
        // TODO: Instead send message to all clientspaces with instructions to create new player
        pendingActions.Enqueue(() => Instantiate(remotePlayerPrefab, Vector3.zero, Quaternion.identity));
    }

    private void HandleHello()
    {
        // Write "someone said hello" to all connected clients
        foreach (ISpace clientSpace in clientSpaces.Values)
        {
            clientSpace.Put("someone said hello!");
        }
    }

    private void RunClientListen()
    {
        Debug.Log("[client] client listen thread started...");
        while (isRunning)
        {
            IEnumerable<ITuple> tuples = mySpace.GetAll(typeof(string));

            foreach (ITuple t in tuples)
            {
                Debug.Log(t[0]);
            }
        }
    }
}
