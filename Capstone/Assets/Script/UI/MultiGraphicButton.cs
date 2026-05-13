using UnityEngine;
using UnityEngine.UI;

/// 替换 Button 组件使用。额外的 Graphic 会跟随按钮状态同步颜色。
public class MultiGraphicButton : Button
{
    [SerializeField] Graphic[] extraGraphics;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        Color c;
        if      (state == SelectionState.Highlighted) c = colors.highlightedColor;
        else if (state == SelectionState.Pressed)     c = colors.pressedColor;
        else if (state == SelectionState.Selected)    c = colors.selectedColor;
        else if (state == SelectionState.Disabled)    c = colors.disabledColor;
        else                                          c = colors.normalColor;

        Color final = c * colors.colorMultiplier;
        float dur = instant ? 0f : colors.fadeDuration;

        foreach (var g in extraGraphics)
            if (g != null) g.CrossFadeColor(final, dur, true, true);
    }
}
