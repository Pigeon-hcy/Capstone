using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LogicalRail))]
public class LogicalRailEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LogicalRail rail = (LogicalRail)target;

        // 按钮：添加固定路径节点
        if (GUILayout.Button("Add Fixed Node"))
        {
            rail.AddNodeFromAssetPath();   // 使用写死路径的方法
        }

        // 按钮：刷新节点
        if (GUILayout.Button("Refresh Nodes From Children"))
        {
            rail.RefreshNodesFromChildren();
        }

        // 按钮：删除所有节点
        if (GUILayout.Button("Clear All Nodes"))
        {
            rail.ClearAllNodes();
        }
    }
}