using UnityEngine;
using QFramework;

namespace SkateGame
{
    public class BasicEnemyController : MonoBehaviour, IController, IAttackable
    {
        public EnemyConfig config;

    private IEnemyModel enemyModel;
    private Rigidbody2D rb;

    // 基于时间的巡逻
    [Header("Time Patrol")]
    float moveDuration = 2f;   // 每段移动持续时间（秒）

    bool  movingRight;
    bool  waiting;
    float moveTimer;
    float waitTimer;

    public bool IsAlive => enemyModel.IsAlive.Value;
    public IArchitecture GetArchitecture() => GameApp.Interface;

    void Start()
    {
        enemyModel = this.GetModel<IEnemyModel>();
        this.GetSystem<IEnemyAssetSystem>().SetEnemyConfig(config);

        rb = GetComponent<Rigidbody2D>();

        enemyModel.Health.Value    = enemyModel.Config.Value.maxHealth;
        enemyModel.MaxHealth.Value = enemyModel.Config.Value.maxHealth;
        enemyModel.IsAlive.Value   = true;

        movingRight = enemyModel.Config.Value.startFacingRight;
        rb.gravityScale = enemyModel.Config.Value.gravityScale;

        // 初始化为移动阶段
        waiting   = false;
        moveTimer = enemyModel.Config.Value.moveDuration; 
    }

    void Update()
        {
            //测试伤害代码
            //if (Input.GetKeyDown(KeyCode.U))
                //TakeDamage(10, DamageType.Physical, null);
            
        if (!enemyModel.IsAlive.Value)
        {
            if (rb) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (waiting)
        {
            // 等待阶段：原地不动，倒计时
            if (rb) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting   = false;
                movingRight = !movingRight;                    // 等完换方向
                moveTimer = Mathf.Max(0.01f, moveDuration);    // 开始下一段移动
            }
            return;
        }

        // 移动阶段：按方向和速度行进
        float speed = enemyModel.Config.Value.moveSpeed * (movingRight ? 1f : -1f);
        if (rb) rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            // 本段移动结束 → 进入等待阶段
            waiting   = true;
            waitTimer = Mathf.Max(0f, enemyModel.Config.Value.waitTime);
            if (rb) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    // ===== IAttackable =====
    public bool TakeDamage(int amount, DamageType type, Vector2? hitPoint)
    {
        if (!enemyModel.IsAlive.Value) return false;

        enemyModel.Health.Value -= amount;
        if (enemyModel.Health.Value <= 0)
        {
            enemyModel.Health.Value = 0;
            enemyModel.IsAlive.Value = false;
            Die();
        }
        return enemyModel.IsAlive.Value;
    }

    void Die()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject, 0.1f);
    }
    }
}

