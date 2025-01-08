using UnityEngine;

using dotSpace.Interfaces.Space;
using dotSpace.Objects.Network;
using dotSpace.Objects.Space;

public class TestA : MonoBehaviour
{

    private SpaceRepository repository;
    private ISpace space;

    void Start()
    {
        repository = new SpaceRepository();
        repository.AddGate("tcp://127.0.0.1:9999?KEEP");

        space = new SequentialSpace();
        repository.AddSpace("space", space);

        Debug.Log("Created at: tcp://127.0.0.1:9999?KEEP");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            ITuple tuple = space.GetP(typeof(string));
            Debug.Log(tuple);
        }
    }

}
