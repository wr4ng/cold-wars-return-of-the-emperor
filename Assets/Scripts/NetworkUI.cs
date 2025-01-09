using TMPro;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [Header("Connect UI")]
    public GameObject connectUI;
    public TMP_InputField hostInputField;
    public TMP_InputField portInputField;

    [Header("In-Game UI")]
    public TMP_Text statusText;

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

    private void Update()
    {
        if (!NetworkManager.isRunning)
        {
            statusText.text = "Status: Not connected";
            return;
        }
        statusText.text = "Status: " + (NetworkManager.isServer ? "Host" : "Client");
    }
}