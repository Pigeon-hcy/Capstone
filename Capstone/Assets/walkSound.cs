using UnityEngine;

public class walkSound : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void _playWalk()
    {
        Debug.Log("Move SFX " + AudioManager.Instance.fmodCheckMove());
        if (AudioManager.Instance.fmodCheckMove())
        {
            AudioManager.Instance.fmodPlayWalk();
        }
    }
    public void _walkTrue()
    {
        AudioManager.Instance.fmodMoveTrue();
        Debug.Log("Walk True");
    }
    public void _walkFalse()
    {
        AudioManager.Instance.fmodMoveFalse();
        Debug.Log("Walk False");
    }
}
