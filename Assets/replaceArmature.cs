using UnityEngine;

public class replaceArmature : MonoBehaviour
{
    public GameObject defaultPlayer;
    public Transform armatureParent;
    public GameObject[] characterPrefabs;

    void Start()
    {
        string selectedCharacterName = characterCreation.selectedCharacterName;

        if (!string.IsNullOrEmpty(selectedCharacterName))
        {
            GameObject selectedCharacterPrefab = null;

            foreach (GameObject prefab in characterPrefabs)
            {
                if (prefab.name == selectedCharacterName)
                {
                    selectedCharacterPrefab = prefab;
                    break;
                }
            }

            if (selectedCharacterPrefab != null)
            {
                foreach (Transform child in armatureParent)
                {
                    Destroy(child.gameObject);
                }
                GameObject newCharacter = Instantiate(selectedCharacterPrefab);
                newCharacter.transform.SetParent(armatureParent, false);
            }
        }
    }
}
