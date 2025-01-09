using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static SpaceRepository repository;
    private static string connectionString;
    private static ISpace serverSpace;

    private static ISpace mySpace;

    public static bool isServer { get; private set; } = false;
    public static bool isRunning { get; private set; } = false;

    private static Thread serverThread;

    public static void StartServer(string host, int port)
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

        isServer = true;
        isRunning = true;
        Debug.Log("Server started with connection string: " + connectionString);
    }

    public static void StartClient(string host, int port)
    {
        connectionString = string.Format("tcp://{0}:{1}/server?KEEP", host, port);
        Debug.Log("Trying to connect to: " + connectionString + "...");
        serverSpace = new RemoteSpace(connectionString);
        serverSpace.Put("join");

        isServer = false;
        isRunning = true;
        Debug.Log("Connected to: " + connectionString);
    }

    public static void Close()
    {
        if (isRunning)
        {
            isRunning = false;
            if (isServer)
            {
                Debug.Log("Closing server...");
                if (serverThread.IsAlive)
                {
                    serverSpace.Put("stop"); // To resolve serverSpace.getAll() call in RunServerListen(). Can maybe also just abort thread?
                }
                serverThread.Join();
                repository.CloseGate(connectionString);
                Debug.Log("Server closed");
            }
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

    private static void RunServerListen()
    {
        Debug.Log("[server] server listen thread started...");
        while (isRunning)
        {
            IEnumerable<ITuple> tuples = serverSpace.GetAll(typeof(string));

            foreach (ITuple t in tuples)
            {
                Debug.Log(t[0]);
            }
        }
    }
}
