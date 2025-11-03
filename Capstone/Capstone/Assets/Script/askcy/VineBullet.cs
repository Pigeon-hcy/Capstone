using UnityEngine;

public class VineBullet : MonoBehaviour
{
    [Header("子弹设置")]
    public float speed = 15f;
    public float lifeTime = 5f;
    
    [Header("方块预制体")]
    public GameObject blockPrefab; // 生成的方块预制体
    
    [Header("方块设置")]
    public float blockSize = 10f; // 方块大小
    public float blockRiseSpeed = 5f; // 方块升起速度
    public float blockRiseHeight = 2f; // 方块升起高度
    public float blockHorizontalSpeed = 8f; // 方块横向移动速度
    public float blockLifetime = 10f; // 方块存在时间
    
    [Header("方块生成设置")]
    public float blockOffset = 0.5f; // 方块生成偏移距
    
    [Header("顶飞设置")]
    public float launchForce = 15f; // 顶飞物体的力度
    public float launchRadius = 2f; // 顶飞影响半径
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private float elapsedTime = 100f;

    
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        elapsedTime = 0f;
        // 设置生命周期
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // 如果没有刚体，使用Transform移动
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }
    
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player") return;
        
        // 检查是否撞击到Ground层（包括地面和墙壁）
        if (IsGroundLayer(other))
        {
           CreateBlockTowardsPlayer();
        }
        
        // 销毁子弹
        Destroy(gameObject);
    }
    


    bool IsGroundLayer(Collider2D collider)
    {
        // 检查是否是Ground层（包括地面和墙壁）
        return collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }
    
    void CreateBlockTowardsPlayer()
    {
        if (blockPrefab == null) return;
        
        // 直接获取玩家位置
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector3 blockDirection = (playerPosition - transform.position).normalized;
        
        // 在子弹位置向玩家方向偏移创建方块
        Vector3 blockPosition = transform.position + blockDirection * blockOffset;
        GameObject block = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
        
        // 设置方块大小
        block.transform.localScale = Vector3.one * blockSize;
        
        // 旋转方块朝向玩家
        float angle = Mathf.Atan2(blockDirection.y, blockDirection.x) * Mathf.Rad2Deg;
        block.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 禁用碰撞器（升起过程中没有碰撞）
        Collider2D blockCollider = block.GetComponent<Collider2D>();
        if (blockCollider != null)
        {
            blockCollider.enabled = false;
        }
        
        // 立即顶飞周围的物体（在生成的那一刻）
        LaunchObjectsAround(blockPosition, blockDirection);
        
        // 给方块一个朝向玩家的初始力
        Rigidbody2D blockRb = block.GetComponent<Rigidbody2D>();
        if (blockRb != null)
        {
            blockRb.AddForce((Vector2)blockDirection * launchForce, ForceMode2D.Impulse);
        }
        
        // 添加方块移动行为
        BlockRiser riser = block.AddComponent<BlockRiser>();
        riser.Initialize(blockRiseSpeed, blockRiseHeight, launchForce, launchRadius, blockLifetime);
 
    }
// 新增方法：立即顶飞周围的物体
void LaunchObjectsAround(Vector3 position, Vector3 blockDirection = default)
{
           Collider2D[] objectsAround = Physics2D.OverlapCircleAll(position, launchRadius);
        
        foreach (Collider2D obj in objectsAround)
        {
            if (obj.gameObject.tag == "Player")
            {
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 使用方块朝向玩家的方向作为顶飞方向
                    Vector2 launchDirection = (Vector2)blockDirection;
                    
                    // 完全重设速度，确保物体被正确顶飞
                    Vector2 newVelocity = launchDirection * launchForce * 0.1f;
                    rb.linearVelocity = newVelocity;
                    
                    // 应用额外的顶飞力
                    rb.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
                }
            }
        }
}

}
