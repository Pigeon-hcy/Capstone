using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutscenePlayer : MonoBehaviour
{
    VideoPlayer videoPlayer;

    public int videoIndex;

    public TextAsset csvFile;
    public List<VideoClip> videoClips = new List<VideoClip>();
    public List<EventReference> fmodEvents = new List<EventReference>();
    List<float> skipTimestamps = new List<float>();

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
        videoPlayer.clip = videoClips[videoIndex];

        ParseCSV();

        #region FMOD

        //Test
        testEventInstance = RuntimeManager.CreateInstance(fmodEvents[videoIndex]);

        #endregion
    }

    void Start()
    {
        videoPlayer.Prepare();
    }

    void ParseCSV()
    {
        if (csvFile == null) return;

        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (videoIndex >= 0 && videoIndex < lines.Length)
        {
            string targetLine = lines[videoIndex];
            string[] values = targetLine.Split(',');

            for (int i = 1; i < values.Length; i++)
            {
                if (float.TryParse(values[i].Trim(), out float time))
                {
                    skipTimestamps.Add(time);
                }
            }
        }
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
            float currentTime = (float)videoPlayer.time;

            foreach (float timestamp in skipTimestamps)
            {
                if (timestamp > currentTime + 0.2f)
                {
                    videoPlayer.time = timestamp;
                    testEventInstance.setTimelinePosition((int)(timestamp * 1000));

                    return; 
                }
            }
        }
    }
}
