using UnityEngine;

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

    public static void LoadIntroScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndex.IntroCrawl);
    }
}
