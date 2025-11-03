using UnityEngine;

public class fireBullet : MonoBehaviour
{
    public GameObject explosionPrefab;
    public GameObject BIGexplosionPrefab;

    [Header("反弹设置")]
    public float bounceForce = 10f; // Y轴反弹力度
    
    [Header("上升气流预制体")]
    public GameObject airCurrentPrefab; // 上升气流预制体（撞击Vine）
    
    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * GetComponent<fireBullet>().GetSpeed();
        }
    }
    
    public float GetSpeed()
    {
        // 如果有刚体，使用刚体速度；否则返回默认速度
        if (rb != null)
        {
            return rb.linearVelocity.magnitude;
        }
        return 15f; // 默认速度
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player") return;
        
        // 检查撞击Ice - 大爆炸
        if (other.gameObject.tag == "Ice")
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                iceExplodeWithoutDestroy(other.gameObject);
                Debug.Log("iceExplodeWithoutDestroy");
            }else{
                iceExplode(other.gameObject);
            }
            return;
        }
        
        // 检查撞击playerHitbox - 反转Y方向
        if (other.gameObject.tag == "PlayerHitBox")
        {
            ReverseYDirection();
            return;
        }
        
        // 检查撞击Vine - 升起上升气流
        if (other.gameObject.tag == "Vine")
        {
            CreateAirCurrent();
            Destroy(other.gameObject);
            explode();
            return;
        }
        
        // 检查是否撞击到Ground层（包括地面和墙壁）
        if (IsGroundLayer(other))
        {
            explode();
        }
    }


    private void explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    private void iceExplode(GameObject other)
    {
        Instantiate(BIGexplosionPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        Destroy(other.gameObject);
    }


    private void iceExplodeWithoutDestroy(GameObject other)
    {
        Instantiate(BIGexplosionPrefab, transform.position, Quaternion.identity);
         Destroy(this.gameObject);
    }

    void CreateAirCurrent()
    {
        if (airCurrentPrefab == null) return;
        
        // 创建上升气流
        Instantiate(airCurrentPrefab, transform.position, Quaternion.identity);
    }
    


    void ReverseYDirection()
    {
        // 反转Y方向
        direction.y = -direction.y;
        
        // 更新刚体速度
        if (rb != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            currentVelocity.y = -currentVelocity.y;
            rb.linearVelocity = currentVelocity;
        }
        
        // 不销毁子弹，让它继续飞行
    }

    bool IsGroundLayer(Collider2D collider)
    {
        // 检查是否是Ground层（包括地面和墙壁）
        return collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }
}
