using dotSpace.Objects.Network;
using UnityEngine;

public class TestB : MonoBehaviour
{
    RemoteSpace space;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            space = new RemoteSpace("tcp://127.0.0.1:9999/space?KEEP");
            Debug.Log("Connected to: tcp://127.0.0.1:9999/space?KEEP");
        }
        if (space != null && Input.GetKeyDown(KeyCode.Space))
        {
            space.Put("hello there!");
        }
    }
}
