using System;
using UnityEngine;

[Serializable]
public enum Character
{
    Default = 0,
    Sidius = 1,
    Snowda = 2,
    Chewbacca = 3,
}

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
    public static Character SelectedCharacter;

    [SerializeField]
    private GameObject[] characters;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple CharacterSelection!");
        }
    }

    private void Update()
    {
        if (NetworkManager.Instance.IsRunning)
            DisableAll();
        else
            EnableSelected();
    }

    public void SelectCharacter(Character c)
    {
        SelectedCharacter = c;
    }

    private void DisableAll()
    {
        foreach (GameObject g in characters)
        {
            g.SetActive(false);
        }
    }

    private void EnableSelected()
    {
        int index = (int)SelectedCharacter;
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(index == i);
        }
    }
}
