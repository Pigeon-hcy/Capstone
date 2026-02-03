using UnityEngine;
using SkateGame;
using QFramework;
public class VineGun : MonoBehaviour, ICanGetSystem, ICanGetModel
{
    [Header("Scripts Ref:")]
    public VineRope grappleRope;

    [Header("Layers Settings:")]
    [SerializeField] private LayerMask grappableLayers; // 可抓钩的Layer遮罩，在Inspector中选择

    [Header("Physics Ref:")]
    public Rigidbody2D m_rigidbody;

    [Header("Shoot Angle:")]
    [SerializeField] private float shootAngleDegrees = 0f; // 世界角度（度），0为向右，90为向上

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20;

    [Header("Rope Lifetime")]
    [SerializeField] private float ropeLifeTime = 5f; // 绳子存在时间，超时后自动断开，0或负值表示不限制
    [SerializeField] private float mutualPullForce = 10f; // 双向拉动时，玩家和物体互相拉动的力
    [SerializeField] private float mutualPullGravityOffTime = 0.5f; // 双向拉动时，重力关闭的持续时间

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;
    private IPlayerModel playerModel;
    private bool isGrappling = false;
    private float ropeTimer = 0f;
    private Rigidbody2D grabbedObjectRb = null; // 被抓住的物体的Rigidbody2D（用于双向拉动）
    private bool isMutualPull = false; // 是否是双向拉动模式
    private float mutualPullTimer = 0f; // 双向拉动计时器

    public IArchitecture GetArchitecture() => GameApp.Interface;

    private void Start()
    {
        grappleRope.enabled = false;
        playerModel = this.GetModel<IPlayerModel>();
    }

    private void Update()
    {



        // 空格：如果未抓取则发射，否则释放（按设定角度发射）
        //if (Input.GetKeyDown(KeyCode.Space))
       // {
          //fireGrabbingHook(shootAngleDegrees);
        //}

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
                    m_rigidbody.gravityScale = playerModel.Config.Value.normalG;
                }
            }
        }
    }

    public void fireGrabbingHook(float Angle)
    {
        shootAngleDegrees = Angle;

        if (grappleRope != null && grappleRope.enabled)
        {
            ReleaseGrapple();
            return;
        }

        SetGrapplePointByAngle();
    }

    void SetGrapplePointByAngle()
    {
        Vector2 direction = GetShootDirectionFromAngle();
        
        RaycastHit2D _hit = Physics2D.Raycast(transform.position, direction, 
            hasMaxDistance ? maxDistnace : Mathf.Infinity, grappableLayers);
        
        if (_hit.collider != null)
        {
            // 检查是否在最大距离内
            if (Vector2.Distance(_hit.point, transform.position) <= maxDistnace || !hasMaxDistance)
            {
                grapplePoint = _hit.point;
                grappleDistanceVector = grapplePoint - (Vector2)transform.position;
                
                // 尝试获取目标刚体，用于双向拉动
                grabbedObjectRb = _hit.rigidbody;
                if (grabbedObjectRb == null)
                {
                    grabbedObjectRb = _hit.transform.GetComponentInParent<Rigidbody2D>();
                    if (grabbedObjectRb == null)
                    {
                        grabbedObjectRb = _hit.transform.GetComponentInChildren<Rigidbody2D>();
                    }
                }

                isMutualPull = grabbedObjectRb != null;
                
                // 只有双向拉动模式才执行（需要刚体）
                if (isMutualPull)
                {
                    var energySystem = this.GetSystem<IEnergySystem>();
                    energySystem.ConsumeEnergy(1);
                    // 保存原始重力值并关闭重力
                    if (m_rigidbody != null)
                    {
                        m_rigidbody.gravityScale = 0f;
                    }
                    
                    // 重置双向拉动计时器
                    mutualPullTimer = 0f;
                    
                    // 双向拉动：玩家与物体互相拉向对方
                    if (mutualPullForce > 0f)
                    {
                        Vector2 toObject = direction.normalized;
                        if (m_rigidbody != null)
                        {
                            m_rigidbody.linearVelocity = Vector2.zero;
                            m_rigidbody.AddForce(toObject * mutualPullForce, ForceMode2D.Impulse);
                        }
                        if (grabbedObjectRb != null)
                        {
                            grabbedObjectRb.AddForce(-toObject * mutualPullForce, ForceMode2D.Impulse);
                        }
                    }
                }
                else
                {
                    // 没有刚体，无法抓取
                    PlayFailedRope(direction);
                    return;
                }
                
                // 启用绳索视觉效果
                grappleRope.enabled = true;
                isGrappling = true;
                ropeTimer = 0f; // 重置寿命计时
            }
            else
            {
                PlayFailedRope(direction);
            }
        }
        else
        {
            PlayFailedRope(direction);
        }
    }

    private void PlayFailedRope(Vector2 direction)
    {
        if (grappleRope == null)
        {
            return;
        }

        float distance = maxDistnace;
        grappleRope.PlayFailedShot(direction, distance);
    }

    private Vector2 GetShootDirectionFromAngle()
    {
        float angle = shootAngleDegrees;
        Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector2.right;
        return direction.normalized;
    }

    public void Grapple()
    {
        // Grapple()保留兼容，当前无需SpringJoint2D
        isGrappling = true;
    }

    public void ReleaseGrapple()
    {
        if (grappleRope != null) grappleRope.enabled = false;
        // 恢复重力
        if (m_rigidbody != null) 
        {
            m_rigidbody.gravityScale = playerModel.Config.Value.normalG;
        }
        isGrappling = false;
        isMutualPull = false;
        grabbedObjectRb = null;
        ropeTimer = 0f;
        mutualPullTimer = 0f;
    }

    public void turnOffRopeVisually()
    {
        if (grappleRope != null) grappleRope.enabled = false;
    }
}
