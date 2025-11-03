using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class ScriptUpdatingDisabler : MonoBehaviour
{
    [MenuItem("Tools/Disable Script Updating")]
    public static void DisableScriptUpdating()
    {
        Debug.Log("请手动禁用Script Updating:");
        Debug.Log("1. 打开 Edit > Project Settings > Player");
        Debug.Log("2. 找到 Other Settings > Script Updating");
        Debug.Log("3. 将其设置为 Disabled");
        Debug.Log("4. 保存设置");
    }
    
    [MenuItem("Tools/Enable Script Updating")]
    public static void EnableScriptUpdating()
    {
        Debug.Log("请手动启用Script Updating:");
        Debug.Log("1. 打开 Edit > Project Settings > Player");
        Debug.Log("2. 找到 Other Settings > Script Updating");
        Debug.Log("3. 将其设置为 Enabled");
        Debug.Log("4. 保存设置");
    }
}
#endif
