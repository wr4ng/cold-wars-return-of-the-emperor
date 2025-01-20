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

    public void SelectCharacter(Character c)
    {
        SelectedCharacter = c;
        int index = (int)c;
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(index == i);
        }
    }
}
