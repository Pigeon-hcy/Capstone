using UnityEngine;
using TMPro;
using QFramework;
using SkateGame;

public class DeviceText : MonoBehaviour, ICanRegisterEvent, IBelongToArchitecture
{
    // 缓存当前输入设备类型，供其他脚本（如 TMPTextRevealBounce）使用
    public static InputDeviceType LastDeviceType { get; private set; } = InputDeviceType.KeyboardMouse;

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
        // 更新当前设备类型缓存
        LastDeviceType = e.DeviceType;

        // 根据设备类型选择原始文本
        string raw = e.DeviceType == InputDeviceType.Gamepad ? gamepadText : keyboardMouseText;

        // 替换按键占位符（例如 {Jump}）
        string jumpKey = e.DeviceType == InputDeviceType.Gamepad ? "A" : "Space";
        string processed = raw.Replace("{Jump}", jumpKey);

        // 将自定义的 {vertexp} 标签转换为 Febucci 使用的 [vertexp] 标签
        processed = processed
            .Replace("{vertexp}", "[vertexp]")
            .Replace("{/vertexp}", "[/vertexp]");

        text.text = processed;
    }
}
