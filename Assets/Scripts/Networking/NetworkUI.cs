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
        NetworkManager.Instance.Close();
        connectUI.SetActive(true);
    }

    private void Update()
    {
        quitButton.gameObject.SetActive(NetworkManager.Instance.IsRunning);
        if (!NetworkManager.Instance.IsRunning)
        {
            statusText.text = "Status: Not connected";
            return;
        }
        statusText.text = "Status: " + (NetworkManager.Instance.IsServer ? "Host" : "Client");
    }
}