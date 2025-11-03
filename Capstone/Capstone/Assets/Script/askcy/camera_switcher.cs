using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class camera_switcher : MonoBehaviour
{
     [Header("摄像机设置")]
    [SerializeField] private CinemachineCamera targetCamera; // 目标摄像机
    [SerializeField] private CinemachineCamera originalCamera; // 原始摄像机
    
    [Header("切换设置")]
    [SerializeField] private bool switchBackOnExit = true; // 离开时是否切换回原始摄像机
    [SerializeField] private float waitTime = 0f; // 等待时间（秒），0表示立即切换
    [SerializeField] private bool changeProjectionMode = false; // 是否切换主相机的投影模式
    [SerializeField] private Camera mainCamera; // 主相机引用
    
    [Header("触发器设置")]
    [SerializeField] private string playerTag = "Player"; // 玩家标签
    
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true; // 是否显示调试信息
    
    private bool playerInTrigger = false; // 玩家是否在触发器中
    private bool isWaiting = false; // 是否正在等待
    private bool hasSwitched = false; // 是否已经切换过摄像机
    private Coroutine waitCoroutine; // 等待协程
    
    void Start()
    {
        // 确保触发器设置为触发器
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().isTrigger = true;
        }
        else
        {
            Debug.LogWarning("Camera Switcher需要附加Collider组件!");
        }
        
        // 如果需要切换投影模式但没有设置主相机，自动查找
        if (changeProjectionMode && mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("启用了投影模式切换但未找到主相机!");
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入的是否是玩家
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = true;
            
            if (showDebugInfo)
                Debug.Log("玩家进入触发器区域");
            
            // 如果已经切换过且设置为离开时切换回，则直接切换回原始摄像机
            if (hasSwitched && switchBackOnExit)
            {
                SwitchToOriginalCamera();
                return;
            }
            
            // 开始等待或立即切换
            if (waitTime > 0f)
            {
                StartWaiting();
            }
            else
            {
                SwitchToTargetCamera();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // 检查离开的是否是玩家
        if (other.CompareTag(playerTag))
        {
            playerInTrigger = false;
            
            if (showDebugInfo)
                Debug.Log("玩家离开触发器区域");
            
            // 如果正在等待，停止等待并重置
            if (isWaiting)
            {
                StopWaiting();
                if (showDebugInfo)
                    Debug.Log("等待被中断，计时器重置");
            }
            
            // 如果设置为离开时切换回原始摄像机
            if (switchBackOnExit && hasSwitched)
            {
                SwitchToOriginalCamera();
            }
        }
    }
    
    /// <summary>
    /// 开始等待计时
    /// </summary>
    private void StartWaiting()
    {
        if (isWaiting) return; // 如果已经在等待，不重复开始
        
        isWaiting = true;
        waitCoroutine = StartCoroutine(WaitAndSwitch());
        
        if (showDebugInfo)
            Debug.Log($"开始等待 {waitTime} 秒后切换摄像机...");
    }
    
    /// <summary>
    /// 停止等待
    /// </summary>
    private void StopWaiting()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        isWaiting = false;
    }
    
    /// <summary>
    /// 等待协程
    /// </summary>
    private IEnumerator WaitAndSwitch()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < waitTime && playerInTrigger)
        {
            elapsedTime += Time.deltaTime;
            
            // 可选：显示倒计时进度
            if (showDebugInfo)
            {
                float remainingTime = waitTime - elapsedTime;
                if (remainingTime > 0.1f) // 避免最后一帧的闪烁
                {
                    Debug.Log($"切换倒计时: {remainingTime:F1} 秒");
                }
            }
            
            yield return null;
        }
        
        // 检查是否还在触发器中（没有被中断）
        if (playerInTrigger)
        {
            isWaiting = false;
            SwitchToTargetCamera();
        }
    }
    
    /// <summary>
    /// 切换到目标摄像机
    /// </summary>
    private void SwitchToTargetCamera()
    {
        if (targetCamera != null)
        {
            // 将目标摄像机的优先级设置为最高
            targetCamera.Priority = 10;
            
            // 可选：降低原始摄像机的优先级
            if (originalCamera != null)
            {
                originalCamera.Priority = 1;
            }
            
            // 如果启用了投影模式切换，将主相机切换到透视模式
            if (changeProjectionMode && mainCamera != null)
            {
                mainCamera.orthographic = false;
                if (showDebugInfo)
                    Debug.Log("主相机切换到透视模式 (Perspective)");
            }
            
            hasSwitched = true;
            
            if (showDebugInfo)
                Debug.Log("切换到目标摄像机: " + targetCamera.name);
        }
        else
        {
            Debug.LogWarning("目标摄像机未设置!");
        }
    }
    
    /// <summary>
    /// 切换回原始摄像机
    /// </summary>
    private void SwitchToOriginalCamera()
    {
        if (originalCamera != null)
        {
            // 将原始摄像机的优先级设置为最高
            originalCamera.Priority = 10;
            
            // 降低目标摄像机的优先级
            if (targetCamera != null)
            {
                targetCamera.Priority = 1;
            }
            
            // 如果启用了投影模式切换，将主相机切换回正交模式
            if (changeProjectionMode && mainCamera != null)
            {
                mainCamera.orthographic = true;
                if (showDebugInfo)
                    Debug.Log("主相机切换回正交模式 (Orthographic)");
            }
            
            hasSwitched = false;
            
            if (showDebugInfo)
                Debug.Log("切换回原始摄像机: " + originalCamera.name);
        }
        else
        {
            Debug.LogWarning("原始摄像机未设置!");
        }
    }
    
    /// <summary>
    /// 重置切换状态
    /// </summary>
    [ContextMenu("重置切换状态")]
    public void ResetSwitchState()
    {
        hasSwitched = false;
        StopWaiting();
        if (showDebugInfo)
            Debug.Log("切换状态已重置");
    }
    
    /// <summary>
    /// 手动切换摄像机（可在Inspector中调用）
    /// </summary>
    [ContextMenu("切换到目标摄像机")]
    public void ManualSwitchToTarget()
    {
        StopWaiting();
        SwitchToTargetCamera();
    }
    
    /// <summary>
    /// 手动切换回原始摄像机（可在Inspector中调用）
    /// </summary>
    [ContextMenu("切换回原始摄像机")]
    public void ManualSwitchToOriginal()
    {
        StopWaiting();
        SwitchToOriginalCamera();
    }
    
    /// <summary>
    /// 获取当前等待进度（0-1）
    /// </summary>
    public float GetWaitProgress()
    {
        if (!isWaiting) return 0f;
        
        // 这里可以添加更精确的进度计算
        return 0f; // 简化版本
    }
    
    /// <summary>
    /// 检查是否正在等待
    /// </summary>
    public bool IsWaiting()
    {
        return isWaiting;
    }
}
