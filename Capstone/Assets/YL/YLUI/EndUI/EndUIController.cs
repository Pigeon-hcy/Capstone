using System.Collections;
using SkateGame;
using UnityEngine;

public class EndUIController : MonoBehaviour
{
    [SerializeField] RectTransform shutterTrans;
    [SerializeField] Animator shutterAnim;
    [SerializeField] EndUICollider endUICollider;
    [SerializeField] GameObject navigator;
    
    private void Start()
    {
        endUICollider.OnTriggerEntered += OnTargetHit;
    }

    private void OnTargetHit(Collider2D other)
    {
        StartCoroutine(StartEndAnimation());
    }

    public void EndSequence_A()
    {
        StartCoroutine(StartEndAnimation());
    }

    public void EndSequence_B(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.disableInput = true;
            playerController.rb.linearVelocity = Vector2.zero;
        }
    }

    public void EndSequence_C()
    {
        navigator.SetActive(true);
        GameStateController.Instance.EnterUIPause();
    }

    IEnumerator StartEndAnimation()
    {
        shutterTrans.gameObject.SetActive(true);
        shutterAnim.Play("anim_EndUI", 0, 0);
        shutterTrans.localScale = Vector3.one * 1.2f;
        //shutterTrans.localRotation = Quaternion.Euler(0, 0, 15f);
        
        float lerpSpeed = 1f;

        while (shutterTrans.localScale.x > 1.001f)
        {
            shutterTrans.localScale = Vector3.Lerp(
                shutterTrans.localScale, 
                Vector3.one, 
                Time.deltaTime * lerpSpeed
            );

            /*
            shutterTrans.localRotation = Quaternion.Lerp(
                shutterTrans.localRotation, 
                Quaternion.identity, 
                Time.deltaTime * lerpSpeed
            );
            */

            yield return null;
        }
        
        shutterTrans.localScale = Vector3.one;
        shutterTrans.localRotation = Quaternion.identity;

        shutterTrans.localScale = Vector3.one;
    }


}
