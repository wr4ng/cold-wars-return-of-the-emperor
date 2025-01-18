using UnityEngine;
using UnityEngine.Video;

public class IntroCrawl : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void Update()
    {
        videoPlayer.playbackSpeed = Input.GetKey(KeyCode.Space) ? 8 : 1;
    }

    private void OnVideoEnd(VideoPlayer _)
    {
        SceneManager.LoadMainScene();
    }
}
