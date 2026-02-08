using System;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    protected List<string> targetTags = new List<string>();
    protected List<Collider2D> triggered = new List<Collider2D>();

    protected Action<Collider2D> onDetect;

    protected bool isActive = false;

    /// <summary>
    /// 开启检测
    /// </summary>
    public void Open(List<string> tags, Action<Collider2D> detectAction)
    {
        targetTags = tags;
        onDetect = detectAction;
        triggered.Clear();
        isActive = true;
        //Debug.LogError("开启了");
    }

    /// <summary>
    /// 关闭检测（不销毁）
    /// </summary>
    public void Close()
    {
        isActive = false;
        ClearTriggered();
    }

    /// <summary>
    /// 清除已经触发过的记录
    /// </summary>
    public void ClearTriggered()
    {
        triggered.Clear();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!isActive)
            return;
        //Debug.LogError("active的");
        if (targetTags == null || targetTags.Count == 0)
            return;
        //Debug.LogError("有tag");

        if (!targetTags.Contains(collision.tag))
            return;
        //Debug.LogError("tag符合");

        if (triggered.Contains(collision))
            return;

        //Debug.LogError("检测了");
        triggered.Add(collision);
        onDetect?.Invoke(collision);
    }
}
