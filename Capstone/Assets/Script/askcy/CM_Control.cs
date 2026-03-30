using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>LateUpdate 需在 CinemachineBrain 之后运行，才能把角偏移叠到真实渲染相机上。</summary>
[DefaultExecutionOrder(200)]
public class CM_Control : MonoBehaviour
{
    [Header("Cinemachine设置")]
    public CinemachineCamera virtualCamera;
    public CinemachinePositionComposer positionComposer;
    [Tooltip("带 CinemachineBrain 的实际渲染相机；留空则用 Camera.main")]
    public Camera outputCamera;
    
    [Header("Z轴锁定")]
    public bool lockZAxis = true; // 是否锁定相机Z轴
    public float lockedZ = -10f; // 锁定的Z轴位置
    
    [Header("镜头跟随设置")]
    public float followSpeed = 2f; // 镜头跟随速度
    public float maxOffset = 3f; // 最大偏移距离
    public float speedMultiplier = 0.5f; // 速度倍数，控制镜头移动的敏感度
    
    [Header("平滑设置")]
    public float smoothTimeX = 0.3f; // X轴平滑时间
    public float smoothTimeY = 0.5f; // Y轴平滑时间
    
    [Header("高度镜头调整设置")]
    public float heightThreshold = 5f; // 高度阈值，超过此高度时开始调整镜头
    public float maxYOffset = 2f; // 最大Y偏移量（向下看）
    public float heightSensitivity = 0.3f; // 高度敏感度，控制Y偏移的响应程度
    public LayerMask groundLayerMask = -1; // 地面层遮罩，用于Raycast检测
    public float raycastDistance = 50f; // Raycast检测距离
    
    private Transform player;
    private Rigidbody2D playerRb;
    private Vector2 originalTargetOffset;
    private Vector2 targetOffset;
    private float velocityX = 0f; // X轴平滑速度
    private float velocityY = 0f; // Y轴平滑速度
    private float currentGroundLevel; // 当前检测到的地面高度
    private float lastValidGroundLevel; // 最后有效的地面高度（备用）

    [Header("CameraDistance动态调整设置")]
    public float stopCameraDistance = 10f; // 停止时的CameraDistance
    public float playerMovingCameraDistance = 15f; // 移动时的CameraDistance
    public float cameraDistanceChangeSpeed = 5f; // CameraDistance变化速度
    public float speedThreshold = 5f; // 速度阈值，超过此速度时切换CameraDistance
    public float slowSpeedDuration = 1.5f; // 速度过慢持续多久后才缩小CameraDistance（秒）
    
    private float targetCameraDistance; // 目标CameraDistance
    private float currentCameraDistance; // 当前CameraDistance
    private float originalCameraDistance; // 原始CameraDistance
    private float slowSpeedTimer = 0f; // 速度过慢的计时器
    private Coroutine wallRideCameraAngleCoroutine;
    /// <summary>叠在 Brain 输出旋转上的局部欧拉角 (0, Y, Z)。</summary>
    private Vector3 wallRideEulerOffset = Vector3.zero;
    private Coroutine grindCameraAngleCoroutine;
    private Vector3 grindEulerOffset = Vector3.zero;

    [Header("Speical Camera position settings")]
    [Header("Speical Camera position settings when wall ride")]
    public float specialCameraPositionYWhenWallRide = 0f;
    public float specialCameraPositionZWhenWallRide = 0f;
    public float EnterTimeWhenWallRide = 0f;
    public float ExitTimeWhenWallRide = 0f;
    [Header("Speical Camera position settings when grind")]
    public float specialCameraPositionYWhenGrind = 0f;
    public float specialCameraPositionZWhenGrind = 0f;

    public float EnterTimeWhenGrind = 0f;
    public float ExitTimeWhenGrind = 0f;

    [Header("Speical Camera position settings when slide Down from the wall(right side)")]
    public float specialCameraPositionYWhenSlideDownFromWallRight = 0f;
    public float specialCameraPositionZWhenSlideDownFromWallRight = 0f;
    [Header("Speical Camera position settings when slide Down from the wall(left side)")]
    public float specialCameraPositionYWhenSlideDownFromWallLeft = 0f;
    public float specialCameraPositionZWhenSlideDownFromWallLeft = 0f;


    


    
    void Start()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;

        // 获取玩家引用
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
        
        // 记录原始的target offset和CameraDistance
        if (positionComposer != null)
        {
            originalTargetOffset = positionComposer.TargetOffset;
            originalCameraDistance = positionComposer.CameraDistance;
        }
        
        // 初始化地面高度参考点
        if (player != null)
        {
            // 使用Raycast检测初始地面高度
            DetectGroundLevel();
            lastValidGroundLevel = currentGroundLevel;
        }
        
        // 初始化CameraDistance
        if (positionComposer != null)
        {
            currentCameraDistance = stopCameraDistance;
            targetCameraDistance = stopCameraDistance;
            positionComposer.CameraDistance = currentCameraDistance;
        }
    }

    void Update()
    {
        if (player == null || positionComposer == null || playerRb == null) return;
        
        // 检测当前地面高度
        DetectGroundLevel();
        
        // 计算镜头偏移
        CalculateCameraOffset();
        
        // 应用镜头偏移
        ApplyCameraOffset();
        
        // 调整CameraDistance
        UpdateCameraDistance();
    }
    
    void LateUpdate()
    {
        // 锁定相机Z轴位置
        if (lockZAxis && virtualCamera != null)
        {
            Transform camTransform = virtualCamera.transform;
            camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y, lockedZ);
        }

        ApplySpecialEulerOffsetsToOutputCamera();
    }

    void ApplySpecialEulerOffsetsToOutputCamera()
    {
        if (outputCamera == null) return;
        if (wallRideEulerOffset.sqrMagnitude < 1e-8f && grindEulerOffset.sqrMagnitude < 1e-8f) return;
        Transform t = outputCamera.transform;
        if (wallRideEulerOffset.sqrMagnitude >= 1e-8f)
            t.rotation = t.rotation * Quaternion.Euler(wallRideEulerOffset);
        if (grindEulerOffset.sqrMagnitude >= 1e-8f)
            t.rotation = t.rotation * Quaternion.Euler(grindEulerOffset);
    }
    
    void CalculateCameraOffset()
    {
        // 获取玩家水平速度
        float horizontalVelocity = playerRb.linearVelocity.x;
        
        // 根据速度计算目标X偏移
        // 玩家往左移动时，镜头往右拉（负速度对应正偏移）
        // 玩家往右移动时，镜头往左拉（正速度对应负偏移）
        float targetXOffset = originalTargetOffset.x + (horizontalVelocity * speedMultiplier);
        
        // 限制X偏移范围
        targetXOffset = Mathf.Clamp(targetXOffset, originalTargetOffset.x - maxOffset, originalTargetOffset.x + maxOffset);
        
        // 计算玩家当前高度（相对于检测到的地面）
        float currentHeight = player.position.y - currentGroundLevel;
        
        // 根据高度计算Y偏移
        float targetYOffset = originalTargetOffset.y;
        if (currentHeight > heightThreshold)
        {
            // 当玩家高度超过阈值时，增加Y偏移（向下看）
            float heightExcess = currentHeight - heightThreshold;
            float yOffsetAdjustment = heightExcess * heightSensitivity;
            targetYOffset = originalTargetOffset.y + Mathf.Clamp(yOffsetAdjustment, 0f, maxYOffset);
        }
        
        // 设置目标偏移
        targetOffset = new Vector2(targetXOffset, targetYOffset);
    }
    
    void ApplyCameraOffset()
    {
        // 获取当前偏移
        Vector2 currentOffset = positionComposer.TargetOffset;
        
        // 分别对X和Y轴进行平滑处理
        float newX = Mathf.SmoothDamp(currentOffset.x, targetOffset.x, ref velocityX, smoothTimeX);
        float newY = Mathf.SmoothDamp(currentOffset.y, targetOffset.y, ref velocityY, smoothTimeY);
        
        // 应用新的偏移
        positionComposer.TargetOffset = new Vector2(newX, newY);
    }
    
    void UpdateCameraDistance()
    {
        if (positionComposer == null || playerRb == null) return;
        
        // 获取玩家速度大小（综合考虑水平和垂直速度）
        float playerSpeed = playerRb.linearVelocity.magnitude;
        
        // 根据速度决定目标CameraDistance
        if (playerSpeed > speedThreshold)
        {
            // 速度超过阈值，立即使用移动CameraDistance
            targetCameraDistance = playerMovingCameraDistance;
            // 重置计时器
            slowSpeedTimer = 0f;
        }
        else
        {
            // 速度低于阈值，开始计时
            slowSpeedTimer += Time.deltaTime;
            
            // 只有当速度持续过慢超过指定时间后，才缩小CameraDistance
            if (slowSpeedTimer >= slowSpeedDuration)
            {
                targetCameraDistance = stopCameraDistance;
            }
            // 否则保持移动CameraDistance
        }
        
        // 使用Lerp平滑过渡到目标CameraDistance
        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetCameraDistance, Time.deltaTime * cameraDistanceChangeSpeed);
        
        // 应用到PositionComposer
        positionComposer.CameraDistance = currentCameraDistance;
    }
    
    void DetectGroundLevel()
    {
        if (player == null) return;
        
        // 从玩家位置向下发射射线
        Vector2 rayOrigin = new Vector2(player.position.x, player.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayerMask);
        
        if (hit.collider != null)
        {
            // 成功检测到地面
            currentGroundLevel = hit.point.y;
            lastValidGroundLevel = currentGroundLevel; // 更新最后有效的地面高度
        }
        else
        {
            // 没有检测到地面，使用最后有效的地面高度
            currentGroundLevel = lastValidGroundLevel;
        }
    }
    
    // 调试用：在Scene视图中显示偏移信息
    void OnDrawGizmosSelected()
    {
        if (player != null && positionComposer != null)
        {
            // 显示原始目标位置
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(player.position + (Vector3)originalTargetOffset, Vector3.one * 0.5f);
            
            // 显示当前目标位置
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(player.position + (Vector3)targetOffset, Vector3.one * 0.3f);
            
            // 显示高度阈值线和地面检测
            if (Application.isPlaying)
            {
                // 显示Raycast射线
                Gizmos.color = Color.cyan;
                Vector3 rayStart = new Vector3(player.position.x, player.position.y, player.position.z);
                Vector3 rayEnd = new Vector3(player.position.x, player.position.y - raycastDistance, player.position.z);
                Gizmos.DrawLine(rayStart, rayEnd);
                
                // 显示检测到的地面高度
                Gizmos.color = Color.magenta;
                Vector3 groundPos = new Vector3(player.position.x, currentGroundLevel, player.position.z);
                Gizmos.DrawLine(groundPos + Vector3.left * 2f, groundPos + Vector3.right * 2f);
                
                // 显示高度阈值线
                Gizmos.color = Color.yellow;
                Vector3 thresholdPos = new Vector3(player.position.x, currentGroundLevel + heightThreshold, player.position.z);
                Gizmos.DrawLine(thresholdPos + Vector3.left * 2f, thresholdPos + Vector3.right * 2f);
                
                // 显示玩家当前高度
                Gizmos.color = Color.green;
                Vector3 currentHeightPos = new Vector3(player.position.x, player.position.y, player.position.z);
                Gizmos.DrawLine(currentHeightPos + Vector3.left * 1f, currentHeightPos + Vector3.right * 1f);
            }
        }
    }

    public void EnterSpecialCameraPositionWhenWallRideLeftToRight()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopWallRideCameraAngleRoutine();
        wallRideCameraAngleCoroutine = StartCoroutine(WallRideEnterCameraAnglesRoutine(
            new Vector3(0f, specialCameraPositionYWhenWallRide, specialCameraPositionZWhenWallRide)));
    }

    /// <summary>与 LeftToRight 相同，但 Y 角为 <c>-specialCameraPositionYWhenWallRide</c>。</summary>
    public void EnterSpecialCameraPositionWhenWallRideRightToLeft()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopWallRideCameraAngleRoutine();
        wallRideCameraAngleCoroutine = StartCoroutine(WallRideEnterCameraAnglesRoutine(
            new Vector3(0f, -specialCameraPositionYWhenWallRide, specialCameraPositionZWhenWallRide)));
    }

    public void ExitSpecialCameraPositionWhenWallRideLeftToRight()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopWallRideCameraAngleRoutine();
        wallRideCameraAngleCoroutine = StartCoroutine(WallRideExitCameraAnglesRoutine());
    }

    public void ExitSpecialCameraPositionWhenWallRideRightToLeft()
    {
        ExitSpecialCameraPositionWhenWallRideLeftToRight();
    }

    void StopWallRideCameraAngleRoutine()
    {
        if (wallRideCameraAngleCoroutine != null)
        {
            StopCoroutine(wallRideCameraAngleCoroutine);
            wallRideCameraAngleCoroutine = null;
        }
    }

    IEnumerator WallRideEnterCameraAnglesRoutine(Vector3 end)
    {
        Vector3 start = wallRideEulerOffset;
        if (EnterTimeWhenWallRide <= 0f)
        {
            wallRideEulerOffset = end;
            wallRideCameraAngleCoroutine = null;
            yield break;
        }
        float duration = EnterTimeWhenWallRide;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.LerpAngle(start.x, end.x, u);
            float y = Mathf.LerpAngle(start.y, end.y, u);
            float z = Mathf.LerpAngle(start.z, end.z, u);
            wallRideEulerOffset = new Vector3(x, y, z);
            yield return null;
        }
        wallRideEulerOffset = end;
        wallRideCameraAngleCoroutine = null;
    }

    IEnumerator WallRideExitCameraAnglesRoutine()
    {
        Vector3 start = wallRideEulerOffset;
        Vector3 end = Vector3.zero;
        if (ExitTimeWhenWallRide <= 0f)
        {
            wallRideEulerOffset = end;
            wallRideCameraAngleCoroutine = null;
            yield break;
        }
        float duration = ExitTimeWhenWallRide;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.LerpAngle(start.x, end.x, u);
            float y = Mathf.LerpAngle(start.y, end.y, u);
            float z = Mathf.LerpAngle(start.z, end.z, u);
            wallRideEulerOffset = new Vector3(x, y, z);
            yield return null;
        }
        wallRideEulerOffset = end;
        wallRideCameraAngleCoroutine = null;
    }

    public void EnterSpecialCameraPositionWhenGrindLeftToRight()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopGrindCameraAngleRoutine();
        grindCameraAngleCoroutine = StartCoroutine(GrindEnterCameraAnglesRoutine(
            new Vector3(0f, specialCameraPositionYWhenGrind, specialCameraPositionZWhenGrind)));
    }

    public void EnterSpecialCameraPositionWhenGrindRightToLeft()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopGrindCameraAngleRoutine();
        grindCameraAngleCoroutine = StartCoroutine(GrindEnterCameraAnglesRoutine(
            new Vector3(0f, -specialCameraPositionYWhenGrind, specialCameraPositionZWhenGrind)));
    }

    public void ExitSpecialCameraPositionWhenGrindLeftToRight()
    {
        if (outputCamera == null)
            outputCamera = Camera.main;
        if (outputCamera == null) return;

        StopGrindCameraAngleRoutine();
        grindCameraAngleCoroutine = StartCoroutine(GrindExitCameraAnglesRoutine());
    }

    public void ExitSpecialCameraPositionWhenGrindRightToLeft()
    {
        ExitSpecialCameraPositionWhenGrindLeftToRight();
    }

    void StopGrindCameraAngleRoutine()
    {
        if (grindCameraAngleCoroutine != null)
        {
            StopCoroutine(grindCameraAngleCoroutine);
            grindCameraAngleCoroutine = null;
        }
    }

    IEnumerator GrindEnterCameraAnglesRoutine(Vector3 end)
    {
        Vector3 start = grindEulerOffset;
        if (EnterTimeWhenGrind <= 0f)
        {
            grindEulerOffset = end;
            grindCameraAngleCoroutine = null;
            yield break;
        }
        float duration = EnterTimeWhenGrind;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.LerpAngle(start.x, end.x, u);
            float y = Mathf.LerpAngle(start.y, end.y, u);
            float z = Mathf.LerpAngle(start.z, end.z, u);
            grindEulerOffset = new Vector3(x, y, z);
            yield return null;
        }
        grindEulerOffset = end;
        grindCameraAngleCoroutine = null;
    }

    IEnumerator GrindExitCameraAnglesRoutine()
    {
        Vector3 start = grindEulerOffset;
        Vector3 end = Vector3.zero;
        if (ExitTimeWhenGrind <= 0f)
        {
            grindEulerOffset = end;
            grindCameraAngleCoroutine = null;
            yield break;
        }
        float duration = ExitTimeWhenGrind;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float x = Mathf.LerpAngle(start.x, end.x, u);
            float y = Mathf.LerpAngle(start.y, end.y, u);
            float z = Mathf.LerpAngle(start.z, end.z, u);
            grindEulerOffset = new Vector3(x, y, z);
            yield return null;
        }
        grindEulerOffset = end;
        grindCameraAngleCoroutine = null;
    }
}
