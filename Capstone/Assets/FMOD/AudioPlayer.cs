using UnityEngine;
using SkateGame;
using FMODUnity;
using FMOD.Studio;
using QFramework;

public class AudioPlayer : MonoBehaviour
{
    public bool playOnLoad;

    void Start()
    {
        Debug.Log("audio player loaded");

        if (playOnLoad)
        {
            //eventInstance.start();
            Debug.Log("playOnLoad is true");
            Debug.Log(AudioManager.Instance);
            playTest();
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
    
    public void playTest()
    {
        AudioManager.Instance.fmodPlayTest();
    }
}
