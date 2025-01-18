using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    private enum SceneIndex {
        IntroCrawl = 0,
        MainScene = 1,
    }

    public static void LoadMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndex.MainScene);
    }
}
