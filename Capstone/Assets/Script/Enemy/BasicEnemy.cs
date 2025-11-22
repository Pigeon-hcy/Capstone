using UnityEngine;
using QFramework;
using System.Net.Security;
using BaseUtility;
using Hitbox;

namespace SkateGame
{

    public enum EnemyMessage
    {
        Die
    }
    public class BasicEnemyController : MonoBehaviour, IController, IAttackable, IInteractable
    {
        public EnemyConfig config;

    [Header("各种工厂")]
    public ReportBoxFactory reportBoxFactory;
    protected IEnemyModel enemyModel;
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

     public bool IfDrawRange = false;
    
    private bool movePaused = false;

    public LayerMask GroundLayer;
    private float timeCount = 0;

    protected IHitBox dmgBox;
    void Start()
    {
        enemyModel = this.GetModel<IEnemyModel>();
        this.GetSystem<IEnemyAssetSystem>().SetEnemyConfig(config);

        rb = GetComponent<Rigidbody2D>();

        enemyModel.Health.Value    = enemyModel.Config.Value.maxHealth;
        enemyModel.MaxHealth.Value = enemyModel.Config.Value.maxHealth;
        enemyModel.IsAlive.Value   = true;
        enemyModel.GuardProcess.Value = 0;
        enemyModel.DetectRadius.Value = enemyModel.Config.Value.detectRadius;
        enemyModel.GuardIncreaseSpeed.Value = enemyModel.Config.Value.guardIncreaseSpeed;
        enemyModel.JumpForce.Value = enemyModel.Config.Value.jumpForce;
        enemyModel.JumpAngleModifier.Value = enemyModel.Config.Value.jumpAngleModifier;
        enemyModel.JumpAtkBoxActiveTime.Value = enemyModel.Config.Value.JumpAtkBoxActiveTime;
        enemyModel.AtkTags.Value  = enemyModel.Config.Value.AtkTags;
        enemyModel.CanBeKilledByQ.Value = enemyModel.Config.Value.canBeKilledByQ;

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

            if(IsPlayerNearWrapper(out Transform trans))
            {
                enemyModel.GuardProcess.Value= Mathf.Clamp01(enemyModel.GuardProcess.Value+enemyModel.GuardIncreaseSpeed.Value/10*Time.deltaTime);   
                //到达1执行跳跃，要可以覆写
                if(enemyModel.GuardProcess.Value == 1)
                {
                    enemyModel.GuardProcess.Value = -0.5f;
                    AtkTowardsPlayer(trans);
                }
            }else
            {
                //警戒降低还要加
                enemyModel.GuardProcess.Value= Mathf.Clamp01(enemyModel.GuardProcess.Value-enemyModel.GuardDecreaseSpeed.Value/10*Time.deltaTime);   
            }

            //Debug.Log(enemyModel.GuardProcess.Value);
            if(!movePaused)
            {
                if (waiting )
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
            }else
            {
                timeCount  -= Time.deltaTime;
                if(timeCount<=0)
                {
                    movePaused = false;
                    if(dmgBox!=null)
                        dmgBox.CloseBox();
                    dmgBox = null;
                }
                   
            }
        
    }

        /// <summary>
        /// 停止移动一段时间，在so里调。停止的时间也是攻击box开启的时间
        /// </summary>
        protected void PauseMove()
        {
            movePaused = true;
            timeCount = enemyModel.JumpAtkBoxActiveTime.Value;
            
        }

        protected virtual void AtkTowardsPlayer(Transform pTrans)
        {
            PauseMove();
            Vector2 CalDir()
            {
                return ((Vector2)pTrans.position - (Vector2)transform.position).normalized;
            }
            Vector2 dir = CalDir();

            float rawAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (rawAngle <= -90f && rawAngle >= -180f)
            {
                rawAngle += 360f;
            }
            //Debug.LogError(" Raw angle"+ rawAngle);

             float offsetFromUp = rawAngle - 90f;

             float result = offsetFromUp * enemyModel.JumpAngleModifier.Value;

            float realResult = result+90f;
            //Debug.LogError(" Jump Result:"+ realResult);

            Vector2 jumpDir = new Vector2(
                Mathf.Cos(realResult * Mathf.Deg2Rad),
                Mathf.Sin(realResult * Mathf.Deg2Rad)
            ).normalized;

            Debug.Log(jumpDir);
            

            float jumpForce = enemyModel.JumpForce.Value*1000;

            rb.AddForce(jumpDir * jumpForce, ForceMode2D.Impulse);
            BoxCollider2D cld = GetComponent<BoxCollider2D>();
            if(dmgBox == null)
            {
                dmgBox = reportBoxFactory.CreateHitbox(transform);
            }
            dmgBox.OpenBox(enemyModel.AtkTags.Value, AtkHandler,cld == null?new Vector2(1,1):cld.size );
            

        }

        public virtual void AtkHandler(GameObject gameObject)
        {
            Debug.Log("PlayerDie!");
            var respawnSystem  = this.GetSystem<IRespawnSystem>();
            if(respawnSystem!=null)
            {
                respawnSystem.RespawnPlayer();
            }
        }

        public void DoInteraction()
        {
            if(enemyModel.CanBeKilledByQ.Value)
            {
                 MessageBox box = new MessageBox();
                box.gmo = this.gameObject;
                MessageSystem.Instance.Send<EnemyMessage>(EnemyMessage.Die, box,this);
                Die();
            }
           
        }

        void OnDrawGizmos()
        {
            if(IfDrawRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, config.detectRadius);
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
        if(dmgBox!= null)
            dmgBox.CloseBox();
        dmgBox = null;
        Destroy(gameObject, 0.1f);
    }

        private Transform player;
        protected bool IsPlayerNearWrapper(out Transform trans)
        {
            trans = null;
            if (player == null)
                player = FindFirstObjectByType<PlayerController>().transform;

            if (player == null)
                return false;

            trans = player;
            return IsPlayerNear2D(transform, player, enemyModel.DetectRadius.Value);
        }

        bool IsPlayerNear2D(Transform self, Transform player, float radius)
        {
            Vector2 a = self.position;
            Vector2 b = player.position;

            return (a - b).sqrMagnitude <= radius * radius;
        }

        public bool IsOnGround(Transform trans, float checkDistance, LayerMask groundMask)
        {
            Vector2 origin = trans.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, checkDistance, groundMask);

            Debug.DrawLine(origin, origin + Vector2.down * checkDistance, hit ? Color.green : Color.red);

            return hit.collider != null;
        }
    }

    
}

