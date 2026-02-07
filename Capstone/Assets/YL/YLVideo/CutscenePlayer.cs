using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    VideoPlayer videoPlayer;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Start()
    {
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer source)
    {
        Time.timeScale = 1f;
        source.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("New_Test_Testforvideo");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayer.time += 10.0f;
        }
    }
}
