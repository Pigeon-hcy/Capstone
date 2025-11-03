using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RailNode))]
public class RailNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 添加 Reset 按钮
        RailNode node = (RailNode)target;
        if (GUILayout.Button("Reset Handle"))
        {
            node.ResetHandle();
        }
    }
}
