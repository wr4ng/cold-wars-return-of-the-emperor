using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [Header("Connect UI")]
    public GameObject connectUI;
    public TMP_InputField hostInputField;
    public TMP_InputField portInputField;

    [Header("In-Game UI")]
    public TMP_Text statusText;
    public Button quitButton;

    public void StartHost()
    {
        NetworkManager.StartServer(hostInputField.text, int.Parse(portInputField.text));
        connectUI.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.StartClient(hostInputField.text, int.Parse(portInputField.text));
        connectUI.SetActive(false);
    }

    public void Quit()
    {
        NetworkManager.Close();
        connectUI.SetActive(true);
    }

    private void Update()
    {
        quitButton.gameObject.SetActive(NetworkManager.isRunning);
        if (!NetworkManager.isRunning)
        {
            statusText.text = "Status: Not connected";
            return;
        }
        statusText.text = "Status: " + (NetworkManager.isServer ? "Host" : "Client");
    }
}