using UnityEngine;
using SkateGame;
using FMODUnity;
using FMOD.Studio;
using QFramework;

public class AudioPlayer : MonoBehaviour
{
    public bool playOnLoad;

    public bool playOnEnter;

    public bool playOnLeave;

    public bool setNewValue;

    public bool pause;

    public bool LM1;

    public bool LM2;

    public bool LM3;

    public bool LM7;

    public float newValue;

    public GameObject speaker;

    void Start()
    {
        Debug.Log("audio player loaded");

        if (speaker == null)
        {
            speaker = GameObject.FindGameObjectWithTag("MusicManager");
            Debug.Log("audio player found");
        }

        if (playOnLoad)
        {
            playConditions();
            Debug.Log("audio play on load");
        }
        else
        {
            Debug.Log("audio not play on load");
        }
    }

    void Update()
    {

    }

    void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playOnEnter)
            {
                playConditions();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playOnLeave)
            {
                playConditions();
            }
        }
    }

    public void playConditions()
    {
        //eventInstance.start();
        Debug.Log("playOnLoad is true");
        Debug.Log(AudioManager.Instance);
        if (setNewValue)
        {
            setNewMusicValue();
            Debug.Log("A");
        }
        else if (pause)
        {
            pauseAudio();
        }
        else
        {
            playAudio();
        }
    }

    public void playAudio()
    {
        if (LM1)
        {
            speaker.GetComponent<MusicManager>().fmodPlayLM1();
        }
        else if (LM2)
        {
            speaker.GetComponent<MusicManager>().fmodPlayLM2();
        }
        else if (LM3)
        {
            speaker.GetComponent<MusicManager>().fmodPlayLM3();
        }
        else if (LM7)
        {
            speaker.GetComponent<MusicManager>().fmodPlayLM7();
        }
    }
    public void pauseAudio()
    {
        if (LM1)
        {
            speaker.GetComponent<MusicManager>().fmodPauseLM1();
        }
        else if (LM2)
        {
            speaker.GetComponent<MusicManager>().fmodPauseLM2();
        }
        else if (LM3)
        {
            speaker.GetComponent<MusicManager>().fmodPauseLM3();
        }
        else if (LM7)
        {
            speaker.GetComponent<MusicManager>().fmodPauseLM7();
        }
    }
    public void setNewMusicValue ()
    {
        if (LM7)
        {
            speaker.GetComponent<MusicManager>().setTargetValue7(newValue);
            Debug.Log("B");
        }
    }
}
