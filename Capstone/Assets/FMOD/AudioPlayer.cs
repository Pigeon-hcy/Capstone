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

    public bool MM;

    public bool LM1;

    public bool LM2;

    public bool LM3;

    public bool LM7;

    public bool LM8;

    public bool LM10;

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
        if (speaker == null)
            return;
        var music = speaker.GetComponent<MusicManager>();
        if (music == null)
            return;
        if (MM)
        {
            music.fmodPlayMainM();
        }
        else if (LM1)
        {
            music.fmodPlayLM1();
        }
        else if (LM2)
        {
            music.fmodPlayLM2();
        }
        else if (LM3)
        {
            music.fmodPlayLM3();
        }
        else if (LM7)
        {
            music.fmodPlayLM7();
        }
        else if (LM8)
        {
            music.fmodPlayLM8();
        }
        else if (LM10)
        {
            music.fmodPlayLM10();
        }
    }
    public void pauseAudio()
    {
        if (speaker == null)
            return;
        var music = speaker.GetComponent<MusicManager>();
        if (music == null)
            return;
        if (MM)
        {
            music.fmodPauseMainM();
        }
        else if (LM1)
        {
            music.fmodPauseLM1();
        }
        else if (LM2)
        {
            music.fmodPauseLM2();
        }
        else if (LM3)
        {
            music.fmodPauseLM3();
        }
        else if (LM7)
        {
            music.fmodPauseLM7();
        }
        else if (LM8)
        {
            music.fmodPauseLM8();
        }
        else if (LM10)
        {
            music.fmodPauseLM10();
        }
    }
    public void setNewMusicValue ()
    {
        if (speaker == null)
            return;
        var music = speaker.GetComponent<MusicManager>();
        if (LM7 && music != null)
        {
            music.setTargetValue7(newValue);
            Debug.Log("B");
        }
    }
}
