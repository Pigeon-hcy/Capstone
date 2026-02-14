using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using FMODUnity;
using FMOD.Studio;

public class CutscenePlayer : MonoBehaviour
{
    VideoPlayer videoPlayer;

    #region FMOD

    //Test
    public EventReference testEvent;
    private EventInstance testEventInstance;
    #endregion
    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;

        #region FMOD

        //Test
        testEventInstance = RuntimeManager.CreateInstance(testEvent);

        #endregion
    }

    void Start()
    {
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer source)
    {
        Time.timeScale = 1f;
        source.Play();
        testEventInstance.start();
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
            testEventInstance.getTimelinePosition(out int currentPos);
            testEventInstance.setTimelinePosition(currentPos + 10000);
        }
    }
}
