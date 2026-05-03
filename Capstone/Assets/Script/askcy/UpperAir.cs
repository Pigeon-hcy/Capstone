using UnityEngine;
using SkateGame;
using QFramework;

public class UpperAir : MonoBehaviour, IBelongToArchitecture, ICanSendEvent
{
    public float forceMagnitude = 45f;
    public Vector2 direction = Vector2.up;
    public GameObject player;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        _playFan();
    }

    void Update ()
    {
        float distance = ((float)Vector3.Distance(this.transform.position, player.transform.position) / 100);
        Debug.Log(distance);
        _setFanDistance(Mathf.Min(_getFanDistance(), distance));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // FOR JERRY'S AUDIO - WIND ACTIVE
        this.SendEvent(new UpperAirEvent
        {
            IsTriggerEnter = true,
            Direction = direction.normalized,
            ForceMagnitude = forceMagnitude
        });
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        this.SendEvent(new UpperAirEvent { IsTriggerEnter = false });
    }

    public void _playFan()
    {
        AudioManager.Instance.fmodPlayFan();
    }

    public float _getFanDistance()
    {
        return AudioManager.Instance.fmodGetFanDistance();
    }

    public void _setFanDistance(float distance)
    {
        AudioManager.Instance.fmodSetFanDistance(distance);
    }

}
