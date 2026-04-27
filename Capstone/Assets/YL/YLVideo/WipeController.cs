using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        image.materialForRendering.SetFloat(circleSizeId, circleSize);
    }

    public void AnimateIn()
    {
        animator.ResetTrigger("Out"); //NOT NEEDED BUT TO FIX BUG
        animator.SetTrigger("In");
    }

    public void AnimateOut()
    {
        animator.ResetTrigger("In");
        animator.SetTrigger("Out");
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AnimateIn();
    }


}
