using MoreMountains.Feedbacks;
using QFramework;
using SkateGame;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour, IBelongToArchitecture, ICanRegisterEvent, ICanSendEvent
{
    Animator animator;
    Image image;
    [SerializeField] GameObject mainButtons;
    [SerializeField] GameObject options;

    bool canInput = true;
    PauseUIState state = PauseUIState.Inactive;

    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] GameObject pauseText;
    [SerializeField] GameObject defaultSelected;
    [SerializeField] GameObject controllerLayout;

    Vector3 bumpAmount;

    enum PauseUIState
    {
        Inactive,
        StartAnimation,
        Active,
        EndAnimation,
    }
    
    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        mainButtons.SetActive(false);
        options.SetActive(false);
        pauseText.SetActive(false);
        image.enabled = false;
        animator.speed = 0;
        bumpAmount = new Vector3(750, 1000, 50);
        InitializeVolumeSliders();
    }

    /*For Test Only
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            canInput = true;
            image.enabled = true;
            state = PauseUIState.StartAnimation;
            animator.speed = 1f;
            animator.enabled = true;
            animator.Play("anim_PauseStart");
            PauseClicked();
        }
    }
    */

    void OnEnable() => this.RegisterEvent<GameStateChangedEvent>(OnGameStateChanged);
    void OnDisable() => this.UnRegisterEvent<GameStateChangedEvent>(OnGameStateChanged);

    // Called by UI Resume/Pause button
    public void PauseClicked() => this.SendEvent<TogglePauseEvent>();

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        if (!canInput) return;
        animator.speed = 1f;
        if (e.NewState == GameState.Pause)
        {
            image.enabled = true;
            state = PauseUIState.StartAnimation;
            animator.speed = 1f;
            animator.enabled = true;
            animator.Play("anim_PauseStart");
            //Pause
            _playPauseMenuOpen();
        }
        else if (e.OldState == GameState.Pause)
        {
            state = PauseUIState.EndAnimation;
            animator.enabled = true;
            animator.Play("anim_PauseEnd");
            //Continue
            _playPauseMenuClose();
        }
    }

    public void EnableInput()
    {
        canInput = true;
        animator.speed = 0;
        if (state == PauseUIState.StartAnimation)
        {
            state = PauseUIState.Active;
        }
        else if (state == PauseUIState.EndAnimation)
        {
            mainButtons.SetActive(false);
            options.SetActive(false);
            pauseText.SetActive(false);
            if (controllerLayout != null) controllerLayout.SetActive(false);
            image.enabled = false;
            state = PauseUIState.Inactive;
        }
    }

    public void DisableInput()
    {
        canInput = false;
    }

    public void StartSpring()
    {
        mainButtons.SetActive(true);
        pauseText.SetActive(true);
        _playPauseMenuClick();
        StartCoroutine(SpringCoroutine());
    }

    public void Options()
    {
        options.SetActive(true);
        _playPauseMenuClick();
        StartCoroutine(OptionsBumpRoutine());
    }

    public void Restart()
    {
        _playPauseMenuClick();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        this.SendEvent(new SceneChangeEvent());
    }

    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator SpringCoroutine()
    {
        pauseText.transform.GetChild(0).GetComponent<MMSpringRectTransformPosition>().Bump(bumpAmount);

        yield return new WaitForSecondsRealtime(0.05f);

        for (int i = 0; i < mainButtons.transform.childCount; i++)
        {
            mainButtons.transform.GetChild(i).GetComponent<MMSpringRectTransformPosition>().Bump(bumpAmount);
            // SFX for button popup
            yield return new WaitForSecondsRealtime(0.05f);
        }

        if (defaultSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(defaultSelected);

        if (controllerLayout != null)
        {
            controllerLayout.SetActive(true);
            controllerLayout.GetComponent<MMSpringScale>().Bump(new Vector3(5f, 5f, 5f));
        }
    }

    IEnumerator OptionsBumpRoutine()
    {
        yield return null;

        if (controllerLayout != null) controllerLayout.SetActive(false);

        MMSpringScale spring = options.GetComponent<MMSpringScale>();
        if (spring != null)
        {
            spring.Bump(new Vector3(20f, 20f, 20f));
        }
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
    public void _playPauseMenuOpen()
    {
        AudioManager.Instance.fmodPlayOpen();
    }
    public void _playPauseMenuClose()
    {
        AudioManager.Instance.fmodPlayClose();
    }
    public void _playPauseMenuClick()
    {
        AudioManager.Instance.fmodPlayClick();
    }
}
