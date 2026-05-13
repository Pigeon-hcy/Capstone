using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(MultiGraphicButton))]
public class MultiGraphicButtonEditor : ButtonEditor
{
    SerializedProperty extraGraphics;

    protected override void OnEnable()
    {
        base.OnEnable();
        extraGraphics = serializedObject.FindProperty("extraGraphics");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(extraGraphics, true);
        serializedObject.ApplyModifiedProperties();
    }
}
