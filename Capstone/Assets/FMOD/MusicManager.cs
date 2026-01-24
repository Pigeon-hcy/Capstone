using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class MusicManager : MonoBehaviour
{
    #region FMOD

    //Test
    public EventReference testEvent;
    private EventInstance testEventInstance;

    //MX1
    public EventReference levelMusic1Event;
    private EventInstance levelMusic1EventInstance;

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
        levelMusic1EventInstance = RuntimeManager.CreateInstance(levelMusic1Event);

        //BMX1
        bossMusic1EventInstance = RuntimeManager.CreateInstance(bossMusic1Event);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {

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
}
