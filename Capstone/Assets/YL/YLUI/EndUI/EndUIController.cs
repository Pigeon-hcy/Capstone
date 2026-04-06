using SkateGame;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EndUIController : MonoBehaviour
{
    [SerializeField] GameObject navigator;
    [SerializeField] EndUICamShutter camShutter;

    Collider2D playerCollider;

    public void EndSequence_A(Collider2D other)
    {
        camShutter.gameObject.SetActive(true);
        StartCoroutine(camShutter.StartEndAnimation());
        playerCollider = other;

        PlayerController playerController = other.GetComponent<PlayerController>();
        playerController.enabled = false;

        StartCoroutine(SlowPlayer());
    }

    public void EndSequence_B() //called by animation event
    {
        Collider2D other = playerCollider;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void EndSequence_C()
    {
        navigator.SetActive(true);
        GameStateController.Instance.EnterUIPause();
        EventSystem.current?.SetSelectedGameObject(navigator);
    }

    IEnumerator SlowPlayer()
    {
        Rigidbody2D rb = playerCollider.attachedRigidbody;

        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            rb.linearVelocity *= 0.99f;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
        }
    } 

}
