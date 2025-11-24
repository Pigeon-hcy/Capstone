using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    #region FMOD
    //Moving
    public EventReference movingEvent;
    private EventInstance movingEventInstance;

    //Ollie
    public EventReference ollieEvent;
    private EventInstance ollieEventInstance;

    //Landing
    public EventReference landEvent;
    private EventInstance landEventInstance;
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        #region FMOD
        //Moving
        movingEventInstance = RuntimeManager.CreateInstance(movingEvent);

        //Ollie
        ollieEventInstance = RuntimeManager.CreateInstance(ollieEvent);

        //Landing
        landEventInstance = RuntimeManager.CreateInstance(landEvent);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Moving
    public void fmodPlayMove()
    {
        if (movingEventInstance.isValid())
        {
            movingEventInstance.start();
        }
    }
    public void fmodPauseMove()
    {
        if (movingEventInstance.isValid())
        {
            movingEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            movingEventInstance.release();
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

    //Landing
    public void fmodPlayLanding ()
    {
        if (landEventInstance.isValid())
        {
            landEventInstance.start();
        }
    }
}
