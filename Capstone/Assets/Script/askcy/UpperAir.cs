using UnityEngine;
using SkateGame;
using QFramework;

public class UpperAir : MonoBehaviour, IBelongToArchitecture, ICanSendEvent
{
    public float forceMagnitude = 45f;
    public Vector2 direction = Vector2.up;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
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
}
