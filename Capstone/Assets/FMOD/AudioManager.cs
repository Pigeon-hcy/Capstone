using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    public Rigidbody2D targetRb;
    float currentSpeed;
    float targetSpeed;

    #region FMOD

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
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;

        if (targetRb != null)
        {
            targetRb = targetRb.GetComponent<Rigidbody2D>();
        }
        currentSpeed = 0f;
        targetSpeed = 0f;

        #region FMOD

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
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        float playerSpeed = Mathf.Abs(targetRb.linearVelocityX);
        //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2);
        currentSpeed = playerSpeed/12;
        //Debug.Log(currentSpeed);
        movingEventInstance.setParameterByID(movingParameter, currentSpeed);
        wallRideEventInstance.setParameterByID(wallRideParameter, currentSpeed);
        railGrindEventInstance.setParameterByID(railGrindParameter, currentSpeed);
    }

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
}
