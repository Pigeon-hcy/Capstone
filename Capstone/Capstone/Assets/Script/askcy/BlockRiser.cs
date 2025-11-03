using UnityEngine;

public class BlockRiser : MonoBehaviour
{
    private float riseSpeed;
    private float riseHeight;
    private float launchForce;
    private float launchRadius;
    private float lifetime;
    private Vector3 moveDirection;
    
    private Vector3 startPosition;
    private bool hasLaunched = false;
    private bool isRising = true;
    private float currentHeight = 0f;
    private Collider2D blockCollider;
    private Rigidbody2D blockRb;
    
    public void Initialize(float speed, float height, float force, float radius, float life, Vector3 direction = default)
    {
        riseSpeed = speed;
        riseHeight = height;
        launchForce = force;
        launchRadius = radius;
        lifetime = life;
        moveDirection = direction == default ? Vector3.up : direction.normalized; // 默认向上，否则使用指定方向
        
        startPosition = transform.position;
        blockCollider = GetComponent<Collider2D>();
        blockRb = GetComponent<Rigidbody2D>();
        
        // 确保碰撞器在升起过程中是禁用的
        if (blockCollider != null)
        {
            blockCollider.enabled = false;
        }
        
        // 设置生命周期
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (isRising && currentHeight < riseHeight)
        {
            // 继续升起，使用指定的移动方向
            currentHeight += riseSpeed * Time.deltaTime;
            transform.position = startPosition + moveDirection * currentHeight;
            
            // 检查是否到达目标高度
            if (currentHeight >= riseHeight)
            {
                // 升起完成，启用碰撞器
                OnRiseComplete();
            }
        }
    }
    
    void OnRiseComplete()
    {
        isRising = false;
        
        // 启用碰撞器，现在方块有刚体碰撞了
        if (blockCollider != null)
        {
            blockCollider.enabled = true;
        }
        
        // 停止刚体运动，让方块稳定下来
        if (blockRb != null)
        {
            blockRb.linearVelocity = Vector2.zero;
            blockRb.angularVelocity = 0f;
        }
        
        // 注意：这里不再发射物体，因为已经在生成时发射了
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, launchRadius);
    }
}
