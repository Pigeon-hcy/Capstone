using UnityEngine;
using TMPro;
using QFramework;
using SkateGame;

public class DeviceText : MonoBehaviour, ICanRegisterEvent, IBelongToArchitecture
{
    [SerializeField] TMP_Text text;

    void Awake()
    {
        if (text == null) text = GetComponent<TMP_Text>();
    }
    [SerializeField] string keyboardMouseText;
    [SerializeField] string gamepadText;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        this.RegisterEvent<InputDeviceSwitchedEvent>(OnDeviceSwitched)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    void OnDeviceSwitched(InputDeviceSwitchedEvent e)
    {
        text.text = e.DeviceType == InputDeviceType.Gamepad ? gamepadText : keyboardMouseText;
    }
}
