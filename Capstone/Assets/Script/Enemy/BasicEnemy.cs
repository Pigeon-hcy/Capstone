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

    public EnemyFactory prefabRef;
    protected IEnemyModel enemyModel;
    private Rigidbody2D rb;
    public Transform displayTrans;

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
    protected virtual void Start()
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
        rb.linearVelocity = Vector2.zero;
        enemyModel.GuardProcess.Register(x =>
        {
            try
            {
                Debug.Log($"{transform.name} 被修改成 {x}");
            }
            catch
            {
            }
        }
        );
        

        // 初始化为移动阶段
        waiting   = false;
        moveTimer = enemyModel.Config.Value.moveDuration; 
    }

    protected struct FlipInfoRecorder
    {
        public bool inGuard;
        public bool moving;
        public bool movingRight;
        public Vector3 playerPos;
    }

    void Update()
    {
        // 测试伤害代码
        // if (Input.GetKeyDown(KeyCode.U))
        //     TakeDamage(10, DamageType.Physical, null);

        if (!enemyModel.IsAlive.Value)
        {
            if (rb) rb.linearVelocity = Vector2.zero;
            return;
        }

        FlipInfoRecorder recorder = new FlipInfoRecorder();

        // --- 警戒逻辑 -------------------------------------------------------
        if (IsPlayerNearWrapper(out Transform trans))
        {
            // 警戒值提升
            enemyModel.GuardProcess.Value =
                Mathf.Clamp01(enemyModel.GuardProcess.Value +
                              enemyModel.GuardIncreaseSpeed.Value / 10f * Time.deltaTime);

            Guard(trans);

            Debug.Log($"{transform.name} 警戒中，当前警戒值 {enemyModel.GuardProcess.Value}，提升速度 {enemyModel.GuardIncreaseSpeed.Value / 10f * Time.deltaTime}");

            // 到达满值 → 跳跃攻击（可覆写）
            if (enemyModel.GuardProcess.Value == 1f)
            {
                enemyModel.GuardProcess.Value = -0.5f;
                AtkTowardsPlayer(trans);
            }

            recorder.inGuard   = true;
            recorder.playerPos = trans.position;
        }
        else
        {
            /*
            Transform p = null;
            if (player == null)
                player = FindFirstObjectByType<PlayerController>()?.transform;

            if (player != null)
            {
                p = player;
                float dist = Vector2.Distance(transform.position, p.position);
                Debug.Log($"{transform.name} 警戒下降中，与玩家距离 = {dist}");
            }
            else
            {
                Debug.Log($"{transform.name} 发现玩家为null ");
            }*/

            // 警戒值下降
            enemyModel.GuardProcess.Value =
                Mathf.Clamp01(enemyModel.GuardProcess.Value -
                              enemyModel.GuardDecreaseSpeed.Value / 10f * Time.deltaTime);

            recorder.inGuard = false;
            UnGuard();
        }
        
        // --------------------------------------------------------------------

        // --- 移动逻辑 -------------------------------------------------------
        if (!movePaused)
        {
            recorder.moving = true;

            // 等待阶段
            if (waiting)
            {
                if (rb) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    waiting       = false;
                    movingRight   = !movingRight;                       // 等完换方向
                    moveTimer     = Mathf.Max(0.01f, moveDuration);      // 开始下一段移动
                }
                return; // 等待中，不进入移动
            }

            // 移动阶段
            float speed = enemyModel.Config.Value.moveSpeed *
                          (movingRight ? 1f : -1f);

            if (rb) rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            recorder.movingRight = movingRight;

            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                // 本段移动结束 → 进入等待阶段
                waiting   = true;
                waitTimer = Mathf.Max(0f, enemyModel.Config.Value.waitTime);

                if (rb) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else
        {
            // 暂停移动（受伤或其它效果）
            timeCount -= Time.deltaTime;
            if (timeCount <= 0f)
            {
                movePaused = false;

                if (dmgBox != null)
                    dmgBox.CloseBox();

                dmgBox = null;
            }
        }
        // --------------------------------------------------------------------

        // 最后更新翻面信息
        Flip(recorder);
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
            dmgBox.OpenBox(enemyModel.AtkTags.Value, new EffectPackage(0),cld == null?new Vector2(1,1):cld.size );
            

        }

        protected virtual void Guard(Transform pTrans)
        {
        }
        
        protected virtual void UnGuard()
        {
        }

        protected virtual void Flip(FlipInfoRecorder recorder)
        {
            if (!recorder.moving)
                return;

            Vector3 scale = displayTrans.localScale;
            
            scale.x = !recorder.movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);

            displayTrans.localScale = scale;
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
                Gizmos.DrawWireSphere(transform.position,enemyModel.DetectRadius.Value);
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

