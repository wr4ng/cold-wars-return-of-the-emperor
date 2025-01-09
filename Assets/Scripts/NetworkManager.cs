using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [SerializeField] public GameObject remotePlayerPrefab;

    private SpaceRepository repository;
    private string connectionString;
    private ISpace serverSpace;

    private int numClients = 0;

    private ISpace mySpace;

    public bool isServer { get; private set; } = false;
    public bool isRunning { get; private set; } = false;

    private Thread serverThread;
    private Thread clientThread;

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
        // Setup server repository
        connectionString = string.Format("tcp://{0}:{1}?KEEP", host, port);
        repository = new SpaceRepository();
        repository.AddGate(connectionString);

        serverSpace = new SequentialSpace();
        repository.AddSpace("server", serverSpace);

        // Start server thread
        serverThread = new Thread(() => RunServerListen());
        serverThread.Start();

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        numClients = 1;

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

        ITuple tuple = serverSpace.Get("players", typeof(int));
        numClients = (int)tuple[1];

        for (int i = 0; i < numClients - 1; i++)
        {
            Instantiate(remotePlayerPrefab, Vector3.zero, Quaternion.identity);
        }

        // Start client thread
        clientThread = new Thread(() => RunClientListen());
        clientThread.Start();

        isServer = false;
        isRunning = true;
        Debug.Log("Connected to: " + connectionString);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            try
            {
                serverSpace.Put(Time.time.ToString());
                Debug.Log("[client] sent timestamp to server");
            }
            catch (SocketException e)
            {
                Debug.Log("[client] failed to send timestamp: " + e.ToString());
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
                if (message == "join")
                {
                    numClients++;
                    serverSpace.Put("players", numClients);
                }
            }
        }
    }

    private void RunClientListen()
    {
        Debug.Log("[client] client listen thread started...");
        while (isRunning)
        {
            IEnumerable<ITuple> tuples = serverSpace.GetAll(typeof(string));

            foreach (ITuple t in tuples)
            {
            }
        }
    }
}
