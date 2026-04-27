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
    public List<string> sceneNames = new List<string>();
    List<float> skipTimestamps = new List<float>();

    [SerializeField] WipeController wipeController;

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
        //videoPlayer.loopPointReached += OnVideoEnd;
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
        wipeController.sceneName = sceneNames[videoIndex];
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

    /*
    void OnVideoEnd(VideoPlayer vp)
    {
        
    }
    */

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float currentTime = (float)videoPlayer.time;

            for (int i = 0; i < skipTimestamps.Count - 1; i++)
            {
                if (skipTimestamps[i] > currentTime + 0.2f)
                {
                    videoPlayer.time = skipTimestamps[i];
                    testEventInstance.setTimelinePosition((int)(skipTimestamps[i] * 1000));

                    return;
                }
            }
        }

        if (videoPlayer.time >= skipTimestamps[skipTimestamps.Count - 1])
        {
            videoPlayer.playbackSpeed = 0;
            wipeController.AnimateOut();
        }
    }
}
