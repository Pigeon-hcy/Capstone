using UnityEngine;

public class VineGun : MonoBehaviour
{
     [Header("Scripts Ref:")]
    public VineRope grappleRope;

    [Header("Layers Settings:")]
    [SerializeField] private LayerMask grappableLayers; // 可抓钩的Layer遮罩，在Inspector中选择
    [SerializeField] private LayerMask pullableObjectLayers; // 双向拉动物体的Layer遮罩，玩家和物体会互相拉向对方

    [Header("Main Camera:")]
    public Camera m_camera;


    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)] [SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;

    [Header("Rope Lifetime")]
    [SerializeField] private float ropeLifeTime = 5f; // 绳子存在时间，超时后自动断开，0或负值表示不限制

    [Header("Swing Settings")]
    [SerializeField] private float swingFrequency = 2f; // 摆荡频率，控制距离关节弹性
    [Range(0f, 1f)] [SerializeField] private float dampingRatio = 0.3f; // 阻尼比，控制摆荡衰减

    [Header("Pull Force")]
    [SerializeField] private float pullForce = 10f; // 朝向抓钩点的拉力
    [SerializeField] private float mutualPullForce = 10f; // 双向拉动时，玩家和物体互相拉动的力
    [SerializeField] private float mutualPullGravityOffTime = 0.5f; // 双向拉动时，重力关闭的持续时间

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;

    private bool isGrappling = false;
    private float ropeTimer = 0f;
    private Rigidbody2D grabbedObjectRb = null; // 被抓住的物体的Rigidbody2D（用于双向拉动）
    private bool isMutualPull = false; // 是否是双向拉动模式
    private float mutualPullTimer = 0f; // 双向拉动计时器
    private float originalGravityScale = 1f; // 保存原始重力值

    private void Start()
    {
        grappleRope.enabled = false;
        if (m_springJoint2D == null)
        {
            m_springJoint2D = GetComponent<SpringJoint2D>();
            if (m_springJoint2D == null)
            {
                m_springJoint2D = gameObject.AddComponent<SpringJoint2D>();
            }
        }
        m_springJoint2D.enabled = false;

        // 保存原始重力值
        if (m_rigidbody != null)
        {
            originalGravityScale = m_rigidbody.gravityScale;
        }
    }

    private void Update()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        RotateGun(mousePos, true);

        // 左键点击：如果未抓取则发射，否则释放
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (grappleRope.enabled || (m_springJoint2D != null && m_springJoint2D.enabled))
            {
                ReleaseGrapple();
            }
            else
            {
                SetGrapplePoint();
            }
        }

        // 绳子生命周期计时
        if (isGrappling && ropeLifeTime > 0f)
        {
            ropeTimer += Time.deltaTime;
            if (ropeTimer >= ropeLifeTime)
            {
                ReleaseGrapple();
            }
        }

        // 双向拉动模式：在拉动期间关闭重力，然后恢复
        if (isMutualPull && mutualPullGravityOffTime > 0f)
        {
            mutualPullTimer += Time.deltaTime;
            if (mutualPullTimer >= mutualPullGravityOffTime)
            {
                // 恢复重力
                if (m_rigidbody != null)
                {
                    m_rigidbody.gravityScale = originalGravityScale;
                }
            }
        }
    }

    void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - transform.position;

        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        if (rotateOverTime && allowRotationOverTime)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void SetGrapplePoint()
    {
        Vector2 mouseWorldPos = (Vector2)m_camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 distanceVector = mouseWorldPos - (Vector2)transform.position;
        
        // 合并两个LayerMask进行射线检测
        LayerMask combinedLayers = grappableLayers | pullableObjectLayers;
        RaycastHit2D _hit = Physics2D.Raycast(transform.position, distanceVector.normalized, 
            hasMaxDistance ? maxDistnace : Mathf.Infinity, combinedLayers);
        
        if (_hit.collider != null)
        {
            // 检查是否在最大距离内
            if (Vector2.Distance(_hit.point, transform.position) <= maxDistnace || !hasMaxDistance)
            {
                grapplePoint = _hit.point;
                grappleDistanceVector = grapplePoint - (Vector2)transform.position;
                
                // 检查是否是双向拉动的layer
                int hitLayer = 1 << _hit.transform.gameObject.layer;
                isMutualPull = (pullableObjectLayers.value & hitLayer) != 0;
                grabbedObjectRb = null;
                
                if (isMutualPull)
                {
                    // 双向拉动模式：直接把玩家拉到物体位置，不使用SpringJoint2D
                    Transform targetObject = _hit.transform;
                    
                    // 找到物体的Rigidbody2D（如果有，用于绳索显示）
                    grabbedObjectRb = _hit.rigidbody;
                    if (grabbedObjectRb == null)
                    {
                        grabbedObjectRb = targetObject.GetComponentInParent<Rigidbody2D>();
                        if (grabbedObjectRb == null)
                        {
                            grabbedObjectRb = targetObject.GetComponentInChildren<Rigidbody2D>();
                        }
                    }
                    
                    // 计算目标位置（物体的位置）
                    Vector2 targetPosition = targetObject.position;
                    
                    // 保存原始重力值并关闭重力
                    if (m_rigidbody != null)
                    {
                        originalGravityScale = m_rigidbody.gravityScale;
                        m_rigidbody.gravityScale = 0f;
                    }
                    
                    // 重置双向拉动计时器
                    mutualPullTimer = 0f;
                    
                    // 给玩家施加朝向物体的强力，直接把玩家拉过去
                    if (m_rigidbody != null && mutualPullForce > 0f)
                    {
                        Vector2 toObject = (targetPosition - (Vector2)transform.position).normalized;
                        m_rigidbody.AddForce(toObject * mutualPullForce, ForceMode2D.Impulse);
                    }
                    
                    // 双向拉动模式下不使用SpringJoint2D，只通过力来拉动
                    // 不启用SpringJoint2D，让玩家可以自由移动到物体位置
                    if (m_springJoint2D != null)
                    {
                        m_springJoint2D.enabled = false;
                    }
                    
                    // 更新grapplePoint为物体位置，用于绳索显示
                    grapplePoint = targetPosition;
                }
                else
                {
                    // 普通模式：玩家被拉向固定点
                    SetupNormalGrapple();
                }
                
                // 启用绳索视觉效果
                grappleRope.enabled = true;
                isGrappling = true;
                ropeTimer = 0f; // 重置寿命计时
            }
        }
    }
    
    // 设置普通抓钩模式（玩家被拉向固定点）
    private void SetupNormalGrapple()
    {
        // 先添加朝向抓钩点的拉力，让玩家开始移动
        if (m_rigidbody != null && pullForce > 0f)
        {
            Vector2 pullDirection = (grapplePoint - (Vector2)transform.position).normalized;
            m_rigidbody.AddForce(pullDirection * pullForce, ForceMode2D.Impulse);
        }
        
        // 然后确定并固定绳子长度
        float ropeLength = Vector2.Distance(transform.position, grapplePoint);
        
        // 配置SpringJoint2D
        if (m_springJoint2D == null)
        {
            m_springJoint2D = gameObject.AddComponent<SpringJoint2D>();
        }
        m_springJoint2D.autoConfigureDistance = false;
        m_springJoint2D.connectedBody = null; // 连接到固定点
        m_springJoint2D.connectedAnchor = grapplePoint;
        m_springJoint2D.distance = ropeLength; // 在点击时确定绳子长度
        m_springJoint2D.frequency = 1f; // 轻微弹性用于摆荡
        m_springJoint2D.dampingRatio = 0.2f;
        m_springJoint2D.enabled = true; // 立即启用，固定玩家
    }

    public void Grapple()
    {
        // Grapple()保留兼容，如果SpringJoint2D被关闭则重新启用
        if (m_springJoint2D != null && !m_springJoint2D.enabled)
        {
            m_springJoint2D.enabled = true;
            isGrappling = true;
        }
    }

    public void ReleaseGrapple()
    {
        if (grappleRope != null) grappleRope.enabled = false;
        if (m_springJoint2D != null) 
        {
            m_springJoint2D.connectedBody = null; // 清除连接
            m_springJoint2D.enabled = false;
        }
        // 恢复重力
        if (m_rigidbody != null) 
        {
            m_rigidbody.gravityScale = originalGravityScale;
        }
        isGrappling = false;
        isMutualPull = false;
        grabbedObjectRb = null;
        ropeTimer = 0f;
        mutualPullTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxDistnace);
        }
    }
}
