using UnityEngine;
using QFramework;
using SkateGame;

public class VineRope : MonoBehaviour, ICanGetSystem, ICanGetModel, ICanSendEvent
{
    #region Settings
    [Header("General References:")]
    public LineRenderer m_lineRenderer;

    [Header("Rope Animation Settings:")]
    [SerializeField] private int percision = 40;
    [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

    [Header("Detection Settings:")]
    [SerializeField] private LayerMask grappableLayers;
    [SerializeField] private float detectionDistance = 1f;
    #endregion

    #region State Machine
    private enum RopeState
    {
        None,
        Extending,
        Grappling,
        Retraction
    }

    private RopeState currentState = RopeState.None;
    #endregion

    #region Private Variables

    private float grappleTime = 0f;
    private Vector2 extendDirection;
    private float currentDistance;
    private Vector2 grapplePoint;
    private IPlayerModel playerModel;
    private IEnergySystem energySystem;
    #endregion

    #region QFramework
    public IArchitecture GetArchitecture() => GameApp.Interface;
    #endregion

    private void Start()
    {
        playerModel = this.GetModel<IPlayerModel>();
        energySystem = this.GetSystem<IEnergySystem>();
    }
    private void Update()
    {
        switch (currentState)
        {
            case RopeState.Extending:
                UpdateExtending();
                UpdateRopeEndPoint();
                break;
            case RopeState.Grappling:
                UpdateGrappling();
                UpdateRopeEndPoint();
                break;
            case RopeState.Retraction:
                UpdateRetraction();
                UpdateRopeEndPoint();
                break;
            default:
                enabled = false;
                break;
        }
    }

    #region States
    
    /// <summary>
    /// 开始延伸绳子, 由VineGun调用
    /// </summary>
    /// <param name="direction">延伸方向</param>
    public void StartExtending(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f || m_lineRenderer == null)
        {
            return;
        }

        currentState = RopeState.Extending;
        extendDirection = direction.normalized;
        currentDistance = 0f;
        m_lineRenderer.positionCount = 2;
        m_lineRenderer.enabled = true;
        enabled = true;
    }

    /// <summary>
    /// 更新延伸状态，检测是否钩住物体
    /// </summary>
    private void UpdateExtending()
    {
        currentDistance += playerModel.Config.Value.extendSpeed * Time.deltaTime;

        if (currentDistance >= playerModel.Config.Value.maxDistance)
        {
            GrappleFailed();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, extendDirection, 
            currentDistance + detectionDistance, grappableLayers);

        if (hit.collider != null)
        {
            StartGrappling(hit);
            return;
        }
    }
    
    /// <summary>
    /// 钩住物体
    /// </summary>
    /// <param name="hit">钩住物体的信息</param>
    private void StartGrappling(RaycastHit2D hit)
    {
        grapplePoint = hit.point;

        this.SendEvent<GrappleEvent>(new GrappleEvent { 
            pullDirection = extendDirection.normalized, 
            IsGrappling = true 
        });
        grappleTime = 0f;
        currentState = RopeState.Grappling;
    }

    /// <summary>
    /// 更新钩住状态，绘制绳子
    /// </summary>
    private void UpdateGrappling()
    {   
        grappleTime += Time.deltaTime;
        if (grappleTime >= playerModel.Config.Value.grappleDuration)
        {
            StartRetraction();
            return;
        }
    }

    /// <summary>
    /// 开始收回绳子
    /// </summary>
    private void StartRetraction()
    {
        if (extendDirection.sqrMagnitude <= 0.0001f || m_lineRenderer == null)
        {
            enabled = false;
            return;
        }
        this.SendEvent<GrappleEvent>(new GrappleEvent { 
            pullDirection = extendDirection.normalized, 
            IsGrappling = false 
        });
        currentState = RopeState.Retraction;
    }

    /// <summary>
    /// 更新收回状态，绘制绳子
    /// </summary>
    private void UpdateRetraction()
    {
        if (currentDistance <= 0f)
        {
            EndRetraction();
            return;
        }

        float delta = playerModel.Config.Value.retractSpeed * Time.deltaTime;
        currentDistance -= delta;

        if (currentDistance <= 0f)
        {
            currentDistance = 0f;
            EndRetraction();
            return;
        }
    }

    /// <summary>
    /// 结束收回绳子
    /// </summary>
    private void EndRetraction()
    {
        currentState = RopeState.None;
        if (m_lineRenderer != null)
        {
            m_lineRenderer.enabled = false;
        }
        enabled = false;
    }
    private void GrappleFailed()
    {
        energySystem.AddEnergy(1);
        StartRetraction();
    }
    
    #endregion

    #region Visualization

    /// <summary>
    /// 更新绳子端点位置（带曲线与逐渐变直动画）
    /// </summary>
    private void UpdateRopeEndPoint()
    {
        if (m_lineRenderer == null) return;

        Vector2 start = transform.position;
        Vector2 end = currentState == RopeState.Grappling ?
            grapplePoint : start + extendDirection * currentDistance;

        bool useCurve = ropeAnimationCurve != null && ropeAnimationCurve.length > 0;
        int pointCount = useCurve ? Mathf.Max(2, percision) : 2;
        m_lineRenderer.positionCount = pointCount;

        if (pointCount == 2)
        {
            m_lineRenderer.SetPosition(0, start);
            m_lineRenderer.SetPosition(1, end);
            return;
        }

        Vector2 dir = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-dir.y, dir.x);

        // 钩住后随时间逐渐变直：用 ropeProgressionCurve 控制弯曲强度 (1→0)
        float straightenProgress = Mathf.Clamp01(grappleTime * straightenLineSpeed);
        float curveStrength = (ropeProgressionCurve != null && ropeProgressionCurve.length > 0)
            ? ropeProgressionCurve.Evaluate(straightenProgress)
            : 1f;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (pointCount > 1) ? ((float)i / (pointCount - 1)) : 0f;
            Vector2 basePoint = Vector2.Lerp(start, end, t);
            float offset = ropeAnimationCurve.Evaluate(t) * StartWaveSize * curveStrength;
            Vector2 point = basePoint + perpendicular * offset;
            m_lineRenderer.SetPosition(i, point);
        }
    }
    #endregion
}
