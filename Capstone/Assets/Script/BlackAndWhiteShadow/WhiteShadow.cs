using SkateGame;
using UnityEngine;
using BaseUtility;

public class WhiteShadow : MessageBehavior
{
    public AudioSource audioSource;
    public AudioClip clip;

    void Start()
    {
        SafeRegister(LevelElementBehaviour.Scream, ScreamHandler);
    }

    public void ScreamHandler(MessageBox box, MonoBehaviour sender)
    {

        audioSource.PlayOneShot(clip);
    }
}

public enum LevelElementBehaviour
{
    Scream,
}
