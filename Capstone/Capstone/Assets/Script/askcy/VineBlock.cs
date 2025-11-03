using UnityEngine;
using MoreMountains.Feedbacks;
using SkateGame;
public class VineBlock : MonoBehaviour
{

    public float lifetime = 10f;
    public MMF_Player OnSpawnEffect;
    public MMF_Player OnDestroyEffect;
    public PhysicsMaterial2D[] physicsMaterials;
    public SpriteRenderer spriteRenderer;

    private bool isIce = false;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // 速度变化相关
    private float originalMaxMoveSpeed;
    private float originalMaxAirHorizontalSpeed;
    private bool speedModified = false;
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        if (OnSpawnEffect != null)
        {
            OnSpawnEffect.PlayFeedbacks();
        }
        if (lifetime > 0)
        {
            Invoke("DestroyObject", lifetime);
        }
        playerController = FindFirstObjectByType<PlayerController>();
    }


void OnTriggerEnter2D(Collider2D other)
{
    if(other.gameObject.tag == "Ice")
    {
        isIce = true;
        gameObject.tag = "Ice";
        col.sharedMaterial = physicsMaterials[1];
        spriteRenderer.color = new Color(0,1,1);
    }
}

void OnCollisionEnter2D(Collision2D other)
{
    if(isIce)
    {
        if(other.gameObject.tag == "Player")
        {
            ApplySpeedBoost(true);
        }
    }
}


void OnCollisionExit2D(Collision2D other)
{
    if(isIce)
    {
        if(other.gameObject.tag == "Player")
        {
            ApplySpeedBoost(false);
        }
    }
}



void ApplySpeedBoost(bool applyBoost)
{
    if (playerController?.playerConfig == null) return;
    
    if (applyBoost && !speedModified)
    {
        // 保存原始速度并翻倍
        originalMaxMoveSpeed = playerController.playerConfig.maxMoveSpeed;
        originalMaxAirHorizontalSpeed = playerController.playerConfig.maxAirHorizontalSpeed;
        
        playerController.playerConfig.maxMoveSpeed *= 2f;
        playerController.playerConfig.maxAirHorizontalSpeed *= 2f;
        
        speedModified = true;
        Debug.Log("玩家速度翻倍！");
    }
    else if (!applyBoost && speedModified)
    {
        // 恢复原始速度
        playerController.playerConfig.maxMoveSpeed = originalMaxMoveSpeed;
        playerController.playerConfig.maxAirHorizontalSpeed = originalMaxAirHorizontalSpeed;
        
        speedModified = false;
        Debug.Log("玩家速度恢复！");
    }
}


    void DestroyObject()
    {
        // 如果方块被销毁时玩家还在上面，恢复速度
        if (speedModified && playerController != null)
        {
            ApplySpeedBoost(false);
        }

        if (OnDestroyEffect != null)
        {
            OnDestroyEffect.PlayFeedbacks();
        }
        Destroy(gameObject);
    }




}
