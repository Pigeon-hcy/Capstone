using UnityEngine;
using SkateGame;
using FMODUnity;
using FMOD.Studio;
using QFramework;

public class AudioPlayer : MonoBehaviour
{
 public bool playOnLoad;

    public bool pause;

    public bool LM;

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
        if (LM)
        {
            speaker.GetComponent<MusicManager>().fmodPlayLM1();
        }
        else
        {
            speaker.GetComponent<MusicManager>().fmodPlayBM1();
        }
    }
    public void pauseAudio()
    {
        if (LM)
        {
            speaker.GetComponent<MusicManager>().fmodPauseLM1();
        }
        else
        {
            speaker.GetComponent<MusicManager>().fmodPauseBM1();
        }
    }
}
