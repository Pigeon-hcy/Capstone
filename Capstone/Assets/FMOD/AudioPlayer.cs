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

    void Start()
    {
        Debug.Log("audio player loaded");

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
            AudioManager.Instance.fmodPlayLM1();
        }
        else
        {
            AudioManager.Instance.fmodPlayBM1();
        }
    }
    public void pauseAudio()
    {
        if (LM)
        {
            AudioManager.Instance.fmodPauseLM1();
        }
        else
        {
            AudioManager.Instance.fmodPauseBM1();
        }
    }
}
