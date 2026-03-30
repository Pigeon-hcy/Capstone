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
        AudioManager.Instance.fmodPlayWalk();
    }
}
