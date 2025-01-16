using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("Play game!");
        SceneManager.LoadScene("MainScene");
    }

    public void CharacterSelect()
    {
        Debug.Log("Character select!");
        // SceneManager.LoadScene("CharacterSelect");
    }

    public void QuitGame()
    {
        Debug.Log("Quit game!");
        Application.Quit();
    }

}

