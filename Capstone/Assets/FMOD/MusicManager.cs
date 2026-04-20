using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class MusicManager : MonoBehaviour
{
    #region FMOD

    //Test
    public EventReference testEvent;
    private EventInstance testEventInstance;

    //MXMain
    public EventReference mainMusicEvent;
    private EventInstance mainMusicEventInstance;

    //MX1
    public EventReference levelMusic1Event;
    private EventInstance levelMusic1EventInstance;

    //MX2
    public EventReference levelMusic2Event;
    private EventInstance levelMusic2EventInstance;

    //MX3
    public EventReference levelMusic3Event;
    private EventInstance levelMusic3EventInstance;

    //MX7
    public EventReference levelMusic7Event;
    private EventInstance levelMusic7EventInstance;
    FMOD.Studio.PARAMETER_ID levelMusic7Parameter;
    public float currentValue7;
    public float targetValue7;

    //MX8
    public EventReference levelMusic8Event;
    private EventInstance levelMusic8EventInstance;

    //MX10
    public EventReference levelMusic10Event;
    private EventInstance levelMusic10EventInstance;


    //BMX1
    public EventReference bossMusic1Event;
    private EventInstance bossMusic1EventInstance;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        #region FMOD

        //Test
        testEventInstance = RuntimeManager.CreateInstance(testEvent);

        //MX1
        mainMusicEventInstance = RuntimeManager.CreateInstance(mainMusicEvent);

        //MX1
        levelMusic1EventInstance = RuntimeManager.CreateInstance(levelMusic1Event);

        //MX2
        levelMusic2EventInstance = RuntimeManager.CreateInstance(levelMusic2Event);

        //MX3
        levelMusic3EventInstance = RuntimeManager.CreateInstance(levelMusic3Event);

        //MX7
        levelMusic7EventInstance = RuntimeManager.CreateInstance(levelMusic7Event);

        FMOD.Studio.EventDescription levelMusic7EventDescription;
        levelMusic7EventInstance.getDescription(out levelMusic7EventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION levelMusic7EventParameterDescription;
        levelMusic7EventDescription.getParameterDescriptionByName("MusicSpeedup", out levelMusic7EventParameterDescription);
        levelMusic7Parameter = levelMusic7EventParameterDescription.id;

        currentValue7 = 1f;
        targetValue7 = 1f;

        //MX8
        levelMusic8EventInstance = RuntimeManager.CreateInstance(levelMusic8Event);

        //MX10
        levelMusic10EventInstance = RuntimeManager.CreateInstance(levelMusic10Event);

        //BMX1
        bossMusic1EventInstance = RuntimeManager.CreateInstance(bossMusic1Event);
        #endregion

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            fmodPlayMainM();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //MX7
        currentValue7 = Mathf.Lerp(currentValue7, targetValue7, Time.deltaTime * 2);
        levelMusic7EventInstance.setParameterByID(levelMusic7Parameter, currentValue7);
    }
    
    #region Music
    //Test
    public void fmodPlayTest()
    {
        if (testEventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            testEventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                testEventInstance.start();
                Debug.Log("play audio test");
            }
            else
            {
                Debug.Log("audio not stopped");
            }
        }
        else
        {
            Debug.Log("audio not valid");
        }
    }

    //Pause All
    public void fmodPauseAll()
    {
        fmodPauseBM1();
        fmodPauseMainM();
        fmodPauseLM1();
        fmodPauseLM2();
        fmodPauseLM3();
        fmodPauseLM7();
    }

    //MainMusic
    public void fmodPlayMainM()
    {
        fmodPauseAll();
        if (mainMusicEventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            mainMusicEventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                mainMusicEventInstance.start();
            }
        }
    }
    public void fmodPauseMainM()
    {
        if (mainMusicEventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            mainMusicEventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                mainMusicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //LevelMusic1
    public void fmodPlayLM1()
    {
        fmodPauseAll();
        if (levelMusic1EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic1EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic1EventInstance.start();
            }
        }
    }
    public void fmodPauseLM1()
    {
        if (levelMusic1EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic1EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic1EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //LevelMusic2
    public void fmodPlayLM2()
    {
        fmodPauseAll();
        if (levelMusic2EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic2EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic2EventInstance.start();
            }
        }
    }
    public void fmodPauseLM2()
    {
        if (levelMusic2EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic2EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic2EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //LevelMusic3
    public void fmodPlayLM3()
    {
        fmodPauseAll();
        if (levelMusic3EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic3EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic3EventInstance.start();
            }
        }
    }
    public void fmodPauseLM3()
    {
        if (levelMusic3EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic3EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic3EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //LevelMusic3
    public void fmodPlayLM7()
    {
        fmodPauseAll();
        if (levelMusic7EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic7EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic7EventInstance.start();
            }
        }
    }
    public void fmodPauseLM7()
    {
        if (levelMusic7EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic7EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic7EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    public void setTargetValue7(float newValue)
    {
        targetValue7 = newValue;
    }

    //LevelMusic8
    public void fmodPlayLM8()
    {
        fmodPauseAll();
        if (levelMusic8EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic8EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic8EventInstance.start();
            }
        }
    }
    public void fmodPauseLM8()
    {
        if (levelMusic8EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic8EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic8EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //LevelMusic10
    public void fmodPlayLM10()
    {
        fmodPauseAll();
        if (levelMusic8EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic10EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                levelMusic10EventInstance.start();
            }
        }
    }
    public void fmodPauseLM10()
    {
        if (levelMusic10EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            levelMusic10EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                levelMusic10EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }

    //BossMusic1
    public void fmodPlayBM1()
    {
        fmodPauseAll();
        if (bossMusic1EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            bossMusic1EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                bossMusic1EventInstance.start();
            }
        }
    }
    public void fmodPauseBM1()
    {
        if (bossMusic1EventInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            bossMusic1EventInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                bossMusic1EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
#endregion
}
