using UnityEngine;
using TMPro;
using Febucci.UI;
using SkateGame;
using QFramework;

/// <summary>
/// 简单封装：对外提供 ShowText(string text)，内部使用 TextAnimator 的 Typewriter 打字效果。
/// 需要在同一个物体上挂 TextAnimator_TMP 和 Typewriter（如 TypewriterByCharacter）组件。
/// </summary>
public class TMPTextRevealBounce : MonoBehaviour, ICanRegisterEvent, IBelongToArchitecture
{
    public InputDeviceType LastDeviceType { get; private set; } = InputDeviceType.KeyboardMouse;
    public TMP_Text text;                    // 可选：如果需要直接访问 TMP_Text
    public TextAnimator_TMP textAnimator;    // 引用 TextAnimator 组件
    public TypewriterByCharacter typewriter;        // 引用 Typewriter 组件（ByCharacter 或 ByWord）

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
    }

    /// <summary>
    /// 对外公开的接口：显示一段文字并用打字机效果播放
    /// </summary>
    /// <param name="content">要显示的文本</param>
    public void ShowText(string content)
    {
        // 组件检查与缓存
        if (textAnimator == null)
        {
            textAnimator = GetComponent<TextAnimator_TMP>();
        }

        if (typewriter == null)
        {
            typewriter = GetComponent<TypewriterByCharacter>();
        }

        if (textAnimator == null || typewriter == null)
        {
            Debug.LogWarning("TMPTextRevealBounce: 缺少 TextAnimator_TMP 或 Typewriter 组件。");
            return;
        }

        bool isKeyboard = LastDeviceType == InputDeviceType.KeyboardMouse;
        bool isPS  = LastDeviceType == InputDeviceType.GamepadPS;
        // use Xbox labels except on PS controller
        string jumpKey = isKeyboard ? "Space"  : (isPS ? "X"  : "A");
        string dashKey = isKeyboard ? "K"      : (isPS ? "○"  : "B");
        string pushKey = isKeyboard ? "Shift"  : (isPS ? "R2" : "RT");
        string hookKey = isKeyboard ? "J"      : (isPS ? "□"  : "X");
        string slamKey = isKeyboard ? "S"      : (isPS ? "L2" : "LT");

        string newText = content
            .Replace("{Jump}", jumpKey)
            .Replace("{Dash}", dashKey)
            .Replace("{Push}", pushKey)
            .Replace("{Hook}", hookKey)
            .Replace("{Slam}", slamKey);

        

        // 通过 Typewriter 的 ShowText 来设置文本并触发打字机
        typewriter.ShowText(newText);
    }
}
