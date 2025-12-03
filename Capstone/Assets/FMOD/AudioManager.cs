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
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        float playerSpeed = Mathf.Abs(targetRb.linearVelocityX);
        //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2);
        currentSpeed = playerSpeed;
        movingEventInstance.setParameterByID(movingParameter, currentSpeed);
        wallRideEventInstance.setParameterByID(wallRideParameter, currentSpeed);
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
    public void fmodPlayLanding()
    {
        if (landEventInstance.isValid())
        {
            landEventInstance.start();
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
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.start();
        }
    }
    public void fmodPauseRailGrind()
    {
        if (wallRideEventInstance.isValid())
        {
            wallRideEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}
