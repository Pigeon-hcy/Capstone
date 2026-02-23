using UnityEngine;
using SkateGame;
using FMODUnity;
using FMOD.Studio;
using QFramework;

public class AudioPlayer : MonoBehaviour
{
 public bool playOnLoad;

    public bool pause;

    public bool LM1;

    public bool LM2;

    public bool LM3;

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
            //eventInstance.start();
            Debug.Log("playOnLoad is true");
            Debug.Log(AudioManager.Instance);
            if (pause)
            {
                pauseAudio();
            }
            else
            {
                playAudio();
            }
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
        if (collision.CompareTag("player"))
        {
            if (pause)
            {
                pauseAudio();
            }
            else if (!playOnLoad)
            {
                playAudio();
            }
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
        else
        {
            speaker.GetComponent<MusicManager>().fmodPlayBM1();
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
        else
        {
            speaker.GetComponent<MusicManager>().fmodPauseBM1();
        }
    }
}
