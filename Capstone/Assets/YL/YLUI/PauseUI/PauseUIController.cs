using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    Animator animator;
    Image image;
    [SerializeField] GameObject buttons;

    bool canInput = true;
    PauseUIState state = PauseUIState.Inactive;

    enum PauseUIState
    {
        Inactive,
        StartAnimation,
        Active,
        EndAnimation,
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        buttons.SetActive(false);
        image.enabled = false;
        animator.speed = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseClicked();
        }
    }

    public void PauseClicked()
    {
        if (canInput)
        {
            animator.speed = 1f;
            if (state == PauseUIState.Inactive)
            {
                Time.timeScale = 0;
                image.enabled = true;
                state = PauseUIState.StartAnimation;
                animator.enabled = true;
                animator.Play("anim_PauseStart");
            }
            else if (state == PauseUIState.Active)
            {
                state = PauseUIState.EndAnimation;
                animator.enabled = true;
                animator.Play("anim_PauseEnd");
            }
        }
    }

    public void EnableInput()
    {
        canInput = true;
        animator.speed = 0;
        if (state == PauseUIState.StartAnimation) 
        {
            buttons.SetActive(true);
            state = PauseUIState.Active;
        } 
        else if (state == PauseUIState.EndAnimation)
        {
            Time.timeScale = 1;
            buttons.SetActive(false);
            image.enabled = false;
            state = PauseUIState.Inactive;
        }
    }

    public void DisableInput()
    {
        canInput = false;
    }
}
