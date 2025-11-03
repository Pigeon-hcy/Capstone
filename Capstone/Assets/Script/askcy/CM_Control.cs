using UnityEngine;
using Unity.Cinemachine;

public class CM_Control : MonoBehaviour
{
    [Header("Cinemachine设置")]
    public CinemachineCamera virtualCamera;
    public CinemachinePositionComposer positionComposer;
    
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
    
    void Start()
    {
        // 获取玩家引用
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
        
        // 记录原始的target offset
        if (positionComposer != null)
        {
            originalTargetOffset = positionComposer.TargetOffset;
        }
        
        // 初始化地面高度参考点
        if (player != null)
        {
            // 使用Raycast检测初始地面高度
            DetectGroundLevel();
            lastValidGroundLevel = currentGroundLevel;
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
}
