using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class WipeController : MonoBehaviour
{
    Animator animator;
    Image image;

    readonly int circleSizeId = Shader.PropertyToID("_CircleSize");

    public float circleSize = 0;
    public string sceneName;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
    }

    void Update()
    {
        image.materialForRendering.SetFloat(circleSizeId, circleSize * 4f);
    }

    public void AnimateIn()
    {
        image.enabled = true;
        animator.ResetTrigger("Out"); //NOT NEEDED BUT TO FIX BUG
        animator.SetTrigger("In");
    }

    public void AnimateOut()
    {
        image.enabled = true;
        animator.ResetTrigger("In");
        animator.SetTrigger("Out");
    }

    public void DisableImage()
    {
        image.enabled = false;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (animator) StartCoroutine(AnimateInWithDelay(1.0f));
    }

    public void TransitionCompleteSoKillYourself()
    {
        Destroy(transform.parent.gameObject);
    }

    IEnumerator AnimateInWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        AnimateIn();
    }
}
