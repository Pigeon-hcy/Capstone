using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private static PauseMenu _instance;
    public static PauseMenu Instance {get => _instance;}

    [Header("音量滑轨")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeVolumeSliders();
    }

    /// <summary>
    /// 初始化音量滑轨
    /// </summary>
    private void InitializeVolumeSliders()
    {
        // 加载已保存的音量设置
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 设置滑轨初始值
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

        // 应用已保存的音量设置到AudioManager
        //if (AudioManager.Instance != null)
        //{
        //    AudioManager.Instance.LoadVolumeSettings();
        //}
    }

    /// <summary>
    /// 音乐音量滑轨变化回调
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    /// <summary>
    /// 音效音量滑轨变化回调
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnDestroy()
    {
        // 移除监听器防止内存泄漏
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
