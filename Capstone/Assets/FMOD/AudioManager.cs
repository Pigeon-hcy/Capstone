using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using QFramework;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{

    public FMOD.Studio.Bus MX;
    public FMOD.Studio.Bus SFX;
    public FMOD.Studio.Bus Master;

    public float MasterVolume = 1f;

    public static AudioManager Instance;

    public Rigidbody2D targetRb;
    float currentSpeed;
    float targetSpeed;

    // 音量控制
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;

    #region FMOD

    //Moving
    public EventReference testEvent;
    private EventInstance testEventInstance;

    //MX1
    public EventReference levelMusic1Event;
    private EventInstance levelMusic1EventInstance;

    //MX2
    public EventReference levelMusic2Event;
    private EventInstance levelMusic2EventInstance;

    //MX3
    public EventReference levelMusic3Event;
    private EventInstance levelMusic3EventInstance;

    //BMX1
    public EventReference bossMusic1Event;
    private EventInstance bossMusic1EventInstance;

    //Moving
    public EventReference movingEvent;
    private EventInstance movingEventInstance;
    FMOD.Studio.PARAMETER_ID movingParameter;

    //Ollie
    public EventReference ollieEvent;
    private EventInstance ollieEventInstance;

    //Kickflip
    public EventReference kickflipEvent;
    private EventInstance kickflipEventInstance;

    //Landing
    public EventReference landEvent;
    private EventInstance landEventInstance;

    //Push
    public EventReference pushEvent;
    private EventInstance pushEventInstance;

    //WallRide
    public EventReference wallRideEvent;
    private EventInstance wallRideEventInstance;
    FMOD.Studio.PARAMETER_ID wallRideParameter;

    //RailGrind
    public EventReference railGrindEvent;
    private EventInstance railGrindEventInstance;
    FMOD.Studio.PARAMETER_ID railGrindParameter;

    //Slam
    public EventReference slamEvent;
    private EventInstance slamEventInstance;

    //Death
    public EventReference deathEvent;
    private EventInstance deathEventInstance;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        Instance = this;
        if (GameObject.FindGameObjectsWithTag("AudioManager").Length <= 1)
        {
            Debug.Log("Instance set");
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("Duplicate destroyed");
        }
        currentSpeed = 0f;
        targetSpeed = 0f;

        #region FMOD

        //Test
        testEventInstance = RuntimeManager.CreateInstance(testEvent);

        //MX1
        levelMusic1EventInstance = RuntimeManager.CreateInstance(levelMusic1Event);

          //MX2
        levelMusic2EventInstance = RuntimeManager.CreateInstance(levelMusic2Event);

        //MX3
        levelMusic3EventInstance = RuntimeManager.CreateInstance(levelMusic3Event);

        //BMX1
        bossMusic1EventInstance = RuntimeManager.CreateInstance(bossMusic1Event);

        //Moving
        movingEventInstance = RuntimeManager.CreateInstance(movingEvent);

        FMOD.Studio.EventDescription movingEventDescription;
        movingEventInstance.getDescription(out movingEventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION movingEventParameterDescription;
        movingEventDescription.getParameterDescriptionByName("Speed", out movingEventParameterDescription);
        movingParameter = movingEventParameterDescription.id;

        //Ollie
        ollieEventInstance = RuntimeManager.CreateInstance(ollieEvent);

        //Kickflip
        kickflipEventInstance = RuntimeManager.CreateInstance(kickflipEvent);

        //Landing
        landEventInstance = RuntimeManager.CreateInstance(landEvent);

        //Push
        pushEventInstance = RuntimeManager.CreateInstance(pushEvent);

        //Wall Ride
        wallRideEventInstance = RuntimeManager.CreateInstance(wallRideEvent);

        FMOD.Studio.EventDescription wallRideEventDescription;
        wallRideEventInstance.getDescription(out wallRideEventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION wallRideEventParameterDescription;
        wallRideEventDescription.getParameterDescriptionByName("Speed", out wallRideEventParameterDescription);
        wallRideParameter = wallRideEventParameterDescription.id;

        //Rail Grind
        railGrindEventInstance = RuntimeManager.CreateInstance(railGrindEvent);

        FMOD.Studio.EventDescription railGrindEventDescription;
        railGrindEventInstance.getDescription(out railGrindEventDescription);
        FMOD.Studio.PARAMETER_DESCRIPTION railGrindEventParameterDescription;
        railGrindEventDescription.getParameterDescriptionByName("Speed", out railGrindEventParameterDescription);
        railGrindParameter = railGrindEventParameterDescription.id;

        //Slam
        slamEventInstance = RuntimeManager.CreateInstance(slamEvent);

        //Death
        deathEventInstance = RuntimeManager.CreateInstance(deathEvent);
        #endregion
    }

    void Start()
    {
        //Buses
        Master = FMODUnity.RuntimeManager.GetBus("bus:/Master");
        MX = FMODUnity.RuntimeManager.GetBus("bus:/Master/MX");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Master/SFX");
    }

    // Update is called once per frame
    void Update()
    {
        //Affect audio changes here
        MX.setVolume(MusicVolume);
        SFX.setVolume(SFXVolume);

        //Find Player Gameobject
        if (targetRb == null)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                targetRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
            }
        }
        else
        {
            float playerSpeed = Mathf.Abs(targetRb.linearVelocityX);
            //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2);
            currentSpeed = playerSpeed / 12;
            //Debug.Log(currentSpeed);
            movingEventInstance.setParameterByID(movingParameter, currentSpeed);
            wallRideEventInstance.setParameterByID(wallRideParameter, currentSpeed);
            railGrindEventInstance.setParameterByID(railGrindParameter, currentSpeed);
        }
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

    //LevelMusic1
    public void fmodPlayLM1()
    {
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

    //BossMusic1
    public void fmodPlayBM1()
    {
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

    #region Player SFX
    //Moving
    public void fmodPlayMove()
    {
        if (movingEventInstance.isValid())
        {
            //FMOD.Studio.PLAYBACK_STATE playbackState;
            //movingEventInstance.getPlaybackState(out playbackState);
            //if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            //{
            movingEventInstance.start();
            //}
            Debug.Log("move");
        }
    }
    public void fmodPauseMove()
    {
        if (movingEventInstance.isValid())
        {
            movingEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Debug.Log("stop move");
        }
    }

    //Ollie
    public void fmodPlayOllie()
    {
        if (ollieEventInstance.isValid())
        {
            ollieEventInstance.start();
        }
    }

    //Kickflip
    public void fmodPlayKickflip()
    {
        if (kickflipEventInstance.isValid())
        {
            kickflipEventInstance.start();
        }
    }

    //Landing
    public void fmodPlayLanding()
    {
        if (landEventInstance.isValid())
        {
            landEventInstance.start();
            Debug.Log("land");
        }
    }

    //Push
    public void fmodPlayPush()
    {
        if (pushEventInstance.isValid())
        {
            pushEventInstance.start();
        }
    }

    //Reverse
    public void fmodPlayReverse()
    {
        if (kickflipEventInstance.isValid())
        {
            kickflipEventInstance.start();
        }
    }

    //Wall Ride
    public void fmodPlayWallRide()
    {
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.start();
        }
    }
    public void fmodPauseWallRide()
    {
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    //Rail Grind
    public void fmodPlayRailGrind()
    {
        if (railGrindEventInstance.isValid())
        {
            railGrindEventInstance.start();
        }
    }
    public void fmodPauseRailGrind()
    {
        if (railGrindEventInstance.isValid())
        {
            railGrindEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    //PowerGrind
    public void fmodPlayPowerGrind()
    {
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.start();
        }
    }
    public void fmodPausePowerGrind()
    {
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    //Slam
    public void fmodPlaySlam()
    {
        if (slamEventInstance.isValid())
        {
            slamEventInstance.start();
        }
    }

    //Death
    public void fmodPlayDeath()
    {
        if (deathEventInstance.isValid())
        {
            deathEventInstance.start();
        }
    }
    #endregion

    #region Volume Control
    /// <summary>
    /// 设置音乐音量 (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 设置音效音量 (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    ///// <summary>
    ///// 加载保存的音量设置
    ///// </summary>
    //public void LoadVolumeSettings()
    //{
    //    musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
    //    sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    //    SetMusicVolume(musicVolume);
    //    SetSFXVolume(sfxVolume);
    //}
    #endregion
}
