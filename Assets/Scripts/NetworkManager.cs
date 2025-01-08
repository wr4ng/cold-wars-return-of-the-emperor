
using UnityEngine;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;
using System.Net.Sockets;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private string host = "127.0.0.1";           // Host to connect to (or create server at)
    [SerializeField, Range(8000, 10000)] private int port = 8042; // The port the server hosts

    private SpaceRepository repository;
    private ISpace serverSpace;

    private ISpace mySpace;

    private bool isServer = false;
    private bool isRunning = false;

    public void StartServer()
    {
        // Setup server repository
        string connectionString = string.Format("tcp://{0}:{1}?KEEP", host, port);
        repository = new SpaceRepository();
        repository.AddGate(connectionString);

        serverSpace = new SequentialSpace();
        repository.AddSpace("server", serverSpace);

        isServer = true;
        isRunning = true;
        Debug.Log("Server started with connection string: " + connectionString);
    }

    public void StartClient()
    {
        string connectionString = string.Format("tcp://{0}:{1}/server?KEEP", host, port);
        serverSpace = new RemoteSpace(connectionString);

        isServer = false;
        isRunning = true;
        Debug.Log("Connected to: " + connectionString);
    }

    private void Update()
    {
        if (!isRunning)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartServer();
                return;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                StartClient();
                return;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isServer)
                {
                    ITuple tuple = serverSpace.GetP(typeof(string));
                    Debug.Log("[server] " + ((tuple == null) ? "null" : tuple[0]));
                }
                else
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
        }
    }
}
