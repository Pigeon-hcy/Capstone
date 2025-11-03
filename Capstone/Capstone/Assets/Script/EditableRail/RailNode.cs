using UnityEngine;

[ExecuteInEditMode]
public class RailNode : MonoBehaviour
{
    public Transform handle;      // 曲线节点的小手柄
    public bool isCurve = false;  // 是否是曲线节点

    void Start()
    {   
        #if UNITY_EDITOR
            // 只在运行时隐藏
            if (Application.isPlaying)
            {
                HideSprites();
            }
        #else
            HideSprites();
        #endif
    }

    private void Reset()
    {
        // 自动绑定第一个子物体为手柄
        if (transform.childCount > 0)
            handle = transform.GetChild(0);

        if (handle != null)
            handle.localPosition = new Vector3(1, 1, 0);
    }

    void OnDrawGizmos()
    {
        if (handle != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, handle.position);
        }
    }

    public void ResetHandle()
    {

        Reset();
    }
    
    public void HideSprites()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        if (handle)
        {
            var handleSr = handle.GetComponent<SpriteRenderer>();
            if (handleSr) handleSr.enabled = false;
        }
    }
}
