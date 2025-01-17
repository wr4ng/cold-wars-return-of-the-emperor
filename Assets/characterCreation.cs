using System.Collections.Generic;
using UnityEngine;

public class characterCreation : MonoBehaviour
{
    private List<GameObject> models;
    private int selectionIndex = 0;
    public static string selectedCharacterName;
    private void Start()
    {
        models = new List<GameObject>();
        foreach (Transform t in transform)
        {
            models.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }

        models[selectionIndex].SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            Select(1);
    }

    public void Select(int index)
    {
        if (index == selectionIndex)
        {
            return;
        }

        if (index < 0 || index >= models.Count)
        {
            return;
        }
        models[selectionIndex].SetActive(false);
        selectionIndex = index;
        models[selectionIndex].SetActive(true);

        selectedCharacterName = models[selectionIndex].name;
    }
}
