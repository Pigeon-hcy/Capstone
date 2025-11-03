using UnityEngine;

public class BlockLauncher : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float launchForce;
    private float launchRadius;
    private float lifetime;
    
    private bool hasLaunched = false;
    
    public void Initialize(Vector3 dir, float spd, float force, float radius, float life)
    {
        direction = dir;
        speed = spd;
        launchForce = force;
        launchRadius = radius;
        lifetime = life;
        
        // 设置生命周期
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // 横向移动
        transform.Translate(direction * speed * Time.deltaTime);
        
        // 如果还没发射过，检查碰撞
        if (!hasLaunched)
        {
            CheckForLaunch();
        }
    }
    
    void CheckForLaunch()
    {
        // 检测周围的物体并顶飞
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(transform.position, launchRadius);
        
        foreach (Collider2D obj in objectsAround)
        {
            if (obj.gameObject != gameObject && obj.gameObject.tag == "Player")
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 计算顶飞方向（与方块移动方向相反）
                    Vector2 launchDirection = new Vector2(-direction.x, direction.y).normalized;
                    if (launchDirection == Vector2.zero)
                    {
                        launchDirection = Vector2.up;
                    }
                    
                    rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
                    hasLaunched = true;
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, launchRadius);
        Gizmos.DrawRay(transform.position, direction * 2f);
    }
}
