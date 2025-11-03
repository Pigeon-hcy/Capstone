using UnityEngine;
using MoreMountains.Feedbacks;

public class explosion : MonoBehaviour
{
    public float explosionForce = 10f;     
    public LayerMask affectedLayer;        
    public MMF_Player OnSpawnEffect;
    
    [Header("生命周期设置")]
    public float lifetime = 0.5f;           // 爆炸持续时间
    public AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 大小变化曲线
    public float maxRadius = 2f;            // 最大碰撞箱半径
    
    private float timer = 0f;               // 计时器
    private CircleCollider2D circleCollider; // 圆形碰撞器引用
    
    private void Start()
    {
        OnSpawnEffect.PlayFeedbacks();
        
        // 获取CircleCollider2D组件
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.radius = 0f; // 初始半径为0
        }
    }

    private void Update()
    {
        // 更新计时器
        timer += Time.deltaTime;
        
        // 计算生命周期进度（0到1）
        float progress = Mathf.Clamp01(timer / lifetime);
        
        // 根据曲线更新碰撞箱大小
        if (circleCollider != null)
        {
            float curveValue = sizeCurve.Evaluate(progress);
            circleCollider.radius = curveValue * maxRadius;
        }
        
        // 生命周期结束后销毁对象
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & affectedLayer) != 0)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                // 直接设置速度，而不是叠加力
                rb.linearVelocity = dir * explosionForce;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }
    }
}
