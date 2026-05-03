using UnityEngine;
using UnityEngine.UI;

public class ButtonMultiTarget : Button
{
    public Graphic[] targetGraphics;

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color targetColor =
            state == SelectionState.Normal ? colors.normalColor :
            state == SelectionState.Highlighted ? colors.highlightedColor :
            state == SelectionState.Pressed ? colors.pressedColor :
            state == SelectionState.Selected ? colors.selectedColor :
            state == SelectionState.Disabled ? colors.disabledColor : Color.white;

        foreach (var graphic in targetGraphics)
        {
            if (graphic != null)
                graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
        }
    }
}
