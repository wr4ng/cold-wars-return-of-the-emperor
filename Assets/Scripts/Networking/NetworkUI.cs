using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public TMP_Text statusText;

    [Header("Connect UI")]
    public GameObject connectUI;
    public TMP_InputField hostInputField;
    public TMP_InputField portInputField;

    [Header("In-Game UI")]
    public GameObject inGameUI;

    private void Update()
    {
        connectUI.SetActive(!NetworkManager.Instance.IsRunning);
        inGameUI.SetActive(NetworkManager.Instance.IsRunning);
        if (!NetworkManager.Instance.IsRunning)
        {
            statusText.text = "Status: Not connected";
            return;
        }
        statusText.text = "Status: " + (NetworkManager.Instance.IsServer ? "Host" : "Client");
    }

    public void StartHost()
    {
        NetworkManager.Instance.StartServer(hostInputField.text, int.Parse(portInputField.text));
        connectUI.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Instance.StartClient(hostInputField.text, int.Parse(portInputField.text));
        connectUI.SetActive(false);
    }

    public void Quit()
    {
        // Close network connection
        NetworkManager.Instance.Close();

        // Close Unity (or stop the editor from playing)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Back()
    {
        // Close network connection
        NetworkManager.Instance.Close();

        // Load initial scene
        SceneManager.LoadIntroScene();
    }

    public void SelectDefault() => CharacterSelection.Instance.SelectCharacter(Character.Default);
    public void SelectSidius() => CharacterSelection.Instance.SelectCharacter(Character.Sidius);
    public void SelectSnowda() => CharacterSelection.Instance.SelectCharacter(Character.Snowda);
    public void SelectChewbacca() => CharacterSelection.Instance.SelectCharacter(Character.Chewbacca);

}