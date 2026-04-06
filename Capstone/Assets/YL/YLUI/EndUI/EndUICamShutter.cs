using UnityEngine;
using System.Collections;

public class EndUICamShutter : MonoBehaviour
{
    [SerializeField] EndUIController endUIController;
    [SerializeField] RectTransform rect;
    [SerializeField] Animator animator;

    public IEnumerator StartEndAnimation()
    {
        animator.Play("anim_EndUI", 0, 0);
        rect.localScale = Vector3.one * 1.2f;

        float lerpSpeed = 1f;

        while (rect.localScale.x > 1.001f)
        {
            rect.localScale = Vector3.Lerp(
                rect.localScale,
                Vector3.one,
                Time.deltaTime * lerpSpeed
            );

            /*
            rect.localRotation = Quaternion.Lerp(
                rect.localRotation, 
                Quaternion.identity, 
                Time.deltaTime * lerpSpeed
            );
            */

            yield return null;
        }

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        rect.localScale = Vector3.one;
    }

    public void OnAnimationShutterClose()
    {
        endUIController.EndSequence_B();
    }

    public void OnAnimationEnd()
    {
        endUIController.EndSequence_C();
    }

    public void PlayAnimation()
    {
        animator.Play("anim_EndUI", 0, 0);
    }
}
