using UnityEngine;
using System;

public class EndUICollider : MonoBehaviour
{
    public event Action<Collider2D> OnTriggerEntered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEntered?.Invoke(other);
    }
}
