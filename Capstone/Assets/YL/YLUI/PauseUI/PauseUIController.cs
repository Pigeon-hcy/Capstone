using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    Animator animator;
    Image image;
    [SerializeField] GameObject mainButtons;
    [SerializeField] GameObject options;

    bool canInput = true;
    PauseUIState state = PauseUIState.Inactive;

    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

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
        mainButtons.SetActive(false);
        options.SetActive(false);
        image.enabled = false;
        animator.speed = 0;

        InitializeVolumeSliders();
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
            mainButtons.SetActive(true);
            options.SetActive(true);
            state = PauseUIState.Active;
        } 
        else if (state == PauseUIState.EndAnimation)
        {
            Time.timeScale = 1;
            mainButtons.SetActive(false);
            image.enabled = false;
            state = PauseUIState.Inactive;
        }
    }

    public void DisableInput()
    {
        canInput = false;
    }

    private void InitializeVolumeSliders()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = savedMusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
