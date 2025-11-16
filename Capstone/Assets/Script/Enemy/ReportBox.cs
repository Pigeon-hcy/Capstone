using UnityEngine;
using System.Collections.Generic;
using QFramework;

public class ReportBox : MonoBehaviour
{

    public delegate void ReportHandler(GameObject obj);
    protected List<string> targetTags = new List<string>();

    protected List<GameObject> reported = new List<GameObject>();

    protected ReportHandler recordHandler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReportBoxOn(List<string> tags,ReportHandler hander, Vector2 scale)
    {
        targetTags = tags;
        reported.Clear();
        recordHandler = hander;
        GetComponent<BoxCollider2D>().size = scale;
    }

    public void ReportBoxClose()
    {
        Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (targetTags == null || targetTags.Count == 0)
            return;

        string colTag = collision.tag;

        // 如果这个 tag 在目标列表里
        if (targetTags.Contains(colTag))
        {
            GameObject obj = collision.gameObject;

            // 防止重复添加
            if (!reported.Contains(obj))
            {
                reported.Add(obj);
                recordHandler?.Invoke(obj);
            }
        }
    }
}
