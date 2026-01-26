using UnityEngine;
using Unity.Cinemachine;

public class OrthSync : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("父透视相机，如果为空则自动获取父物体的相机")]
    public Camera perspectiveCamera;
    
    [Tooltip("当前正交相机，如果为空则自动获取本物体的相机")]
    public Camera orthographicCamera;
    
    [Header("Sync Settings")]
    [Tooltip("同步距离（正交相机应该模拟的深度）。如果启用Cinemachine同步，将从激活的Cinemachine相机读取CameraDistance")]
    public float syncDistance = 10f;
    
    [Tooltip("是否从Cinemachine读取CameraDistance并同步")]
    public bool syncFromCinemachine = true;
    
    [Tooltip("是否每帧同步（如果父相机FOV会动态改变则开启）")]
    public bool continuousSync = true;
    
    private CinemachineBrain cinemachineBrain;

    void Start()
    {
        // 自动获取相机引用
        if (orthographicCamera == null)
        {
            orthographicCamera = GetComponent<Camera>();
        }
        
        if (perspectiveCamera == null && transform.parent != null)
        {
            perspectiveCamera = transform.parent.GetComponent<Camera>();
        }
        
        // 验证
        if (orthographicCamera == null)
        {
            Debug.LogError("OrthSync: 未找到正交相机组件！");
            enabled = false;
            return;
        }
        
        if (perspectiveCamera == null)
        {
            Debug.LogError("OrthSync: 未找到父透视相机！");
            enabled = false;
            return;
        }
        
        if (!orthographicCamera.orthographic)
        {
            Debug.LogWarning("OrthSync: 当前相机不是正交模式，已自动设置为正交模式");
            orthographicCamera.orthographic = true;
        }
        
        // 如果启用Cinemachine同步，查找CinemachineBrain
        if (syncFromCinemachine)
        {
            cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();
            if (cinemachineBrain == null)
            {
                Debug.LogWarning("OrthSync: 未找到CinemachineBrain，将使用默认的syncDistance值");
            }
        }
        
        // 初始同步
        SyncCameras();
    }

    void LateUpdate()
    {
        if (continuousSync)
        {
            SyncCameras();
        }
    }

    /// <summary>
    /// 同步相机参数
    /// </summary>
    void SyncCameras()
    {
        if (perspectiveCamera == null || orthographicCamera == null)
            return;
        
        // 如果启用Cinemachine同步，尝试从当前激活的Cinemachine相机读取CameraDistance
        if (syncFromCinemachine && cinemachineBrain != null)
        {
            CinemachineCamera activeCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
            if (activeCamera != null)
            {
                // 获取CinemachinePositionComposer组件并读取CameraDistance
                CinemachinePositionComposer positionComposer = activeCamera.GetComponent<CinemachinePositionComposer>();
                if (positionComposer != null)
                {
                    syncDistance = positionComposer.CameraDistance;
                }
                else
                {
                    // 如果找不到PositionComposer，尝试从Lens的PhysicalProperties.FocusDistance获取（作为备选）
                    if (activeCamera.Lens.PhysicalProperties.FocusDistance > 0)
                    {
                        syncDistance = activeCamera.Lens.PhysicalProperties.FocusDistance;
                    }
                }
            }
        }
        
        // 根据透视相机的FOV计算正交相机的大小
        // 公式: orthographicSize = distance * tan(FOV/2)
        float halfFOV = perspectiveCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        orthographicCamera.orthographicSize = syncDistance * Mathf.Tan(halfFOV);
        
        // 同步其他参数
        orthographicCamera.nearClipPlane = perspectiveCamera.nearClipPlane;
        orthographicCamera.farClipPlane = perspectiveCamera.farClipPlane;
        
        // 同步位置和旋转（如果需要的话）
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    
    /// <summary>
    /// 手动触发同步（可以从外部调用）
    /// </summary>
    public void ManualSync()
    {
        SyncCameras();
    }
}
