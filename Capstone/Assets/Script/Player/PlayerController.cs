using UnityEngine;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using System;
using Unity.VisualScripting;

/*
/                       _oo0oo_
/                      o8888888o
/                      88" . "88
/                      (| -_- |)
/                      0\  =  /0
/                    ___/`---'\___
/                  .' \\|     |// '.
/                 / \\|||  :  |||// \
/                / _||||| -:- |||||- \
/               |   | \\\  -  /// |   |
/               | \_|  ''\---/''  |_/ |
/               \  .-\__  '-'  ___/-. /
/             ___'. .'  /--.--\  `. .'___
/          ."" '<  `.___\_<|>_/___.' >' "".
/         | | :  `- \`.;`\ _ /`;.`/ - ` : | |
/         \  \ `_.   \_ __\ /__ _/   .-` /  /
/     =====`-.____`.___ \_____/___.-`___.-'=====
/                       `=---='
*/
/******************************************************/
namespace SkateGame
{
    /// <summary>
    /// 玩家控制器
    /// 集成了输入检测、状态机管理和物理控制
    /// 替代原来的PlayerScript
    /// </summary>
    public class PlayerController : ViewerControllerBase
    {
        public PlayerConfig playerConfig;
        private IPlayerModel playerModel;
        private IPlayerSystem playerSystem;
        private ICollisionSystem collisionSystem;

        [Header("Collision")]
        public Transform bottomLeft;
        public Transform bottomRight;
        public Collider2D bodyCollider;
        [Header("Hitbox")]
        public Collider2D slamHitbox;
        Coroutine _slamHitboxRoutine;
        Coroutine _clearTricksRoutine;

        [Header("Grappling Vine Gun")]
        public VineGun vineGun;

        [Header("状态机")]
        public LayeredStateMachine stateMachine;
        public Rigidbody2D rb;

        [Header("Animation")]
        public Animator animator;
        private GameObject sprite => animator.gameObject;

        [Header("瞄准与射击设置")]
        public LineRenderer aimLine;      // 瞄准线 temp
        public float aimLineLength = 10f; // 瞄准线长度

        [Header("Trigger 控制")]
        public bool disableInput = false;

        [Header("MMF效果")]
        public MMF_Player moveEffect;
        public MMF_Player powerGrindEffect;
        public MMF_Player ReverseEffect;
        public MMF_Player GrindEffect;
        public MMF_Player WallRideEffect;
        public MMF_Player TrickAEffect;
        public MMF_Player TrickBEffect;
        public MMF_Player TrickABoostEffect;
        public MMF_Player TrickBBoostEffect;
        public MMF_Player AirEffect;
        public MMF_Player JumpEffect;
        public MMF_Player GJumpEffect;
        public MMF_Player DoubleJumpEffect;
        public MMF_Player GrabEffect;
        public MMF_Player IdleEffect;
        public MMF_Player powerGrindEffectPlayer;
        public MMF_Player landEffectPlayer;
        public MMF_Player pushEffectPlayer;


        [Header("粒子特效容器")]
        public Transform particleEffectContainer; // 粒子特效容器

        [Header("死亡路径追踪")]
        [Tooltip("用于显示死亡路径的点预制体，留空则不显示轨迹")]
        public GameObject tracePointPrefab;
        [Tooltip("死亡瞬间位置用的预制体（如骷髅、叉等），留空则用路径点预制体")]
        public GameObject traceDeathPointPrefab;
        private ITraceSystem traceSystem;
        private float traceTimer;
        private const float TraceInterval = 0.1f;
        private readonly List<GameObject> spawnedTracePoints = new List<GameObject>();

        protected override void InitializeController()
        {
            // 获取玩家参数
            playerModel = this.GetModel<IPlayerModel>();
            playerSystem = this.GetSystem<IPlayerSystem>();
            collisionSystem = this.GetSystem<ICollisionSystem>();
            traceSystem = this.GetSystem<ITraceSystem>();
            this.GetSystem<IPlayerAssetSystem>().SetPlayerConfig(playerConfig);

            // 获取组件
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            playerModel.CurrentGravityScale.Value = playerModel.Config.Value.normalG;

            // 初始化瞄准时间
            playerModel.MaxAimTime.Value = playerModel.Config.Value.baseMaxAimTime;
            
            // 初始化瞄准线 - 确保在第一次瞄准前不显示
            if (aimLine != null) 
            {
                aimLine.enabled = false;
            }

            // 初始化分层状态机
            stateMachine = new LayeredStateMachine();

            // Get Animation Duration
            playerModel.JumpDuration.Value = AnimationDuratioSetter.GetClipLength(animator, "oPlayer@Ollie");
            playerModel.DoubleJumpDuration.Value = AnimationDuratioSetter.GetClipLength(animator, "oPlayer@KickFlip");
            playerModel.LandDuration.Value = AnimationDuratioSetter.GetClipLength(animator, "oPlayer@OllieLand");
            playerModel.DoubleJumpLandDuration.Value = AnimationDuratioSetter.GetClipLength(animator, "oPlayer@KickFlipLand");
            playerModel.PushDuration.Value = AnimationDuratioSetter.GetClipLength(animator, "oPlayer@Push");

            // Movement Layer
            stateMachine.AddState(new IdleState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new JumpState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new MoveState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new AirState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new WallJumpState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new WallSlideState(this, rb), StateLayer.Movement);
            // stateMachine.AddState(new DoubleJumpState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new ReverseState(this, rb), StateLayer.Movement);
            stateMachine.AddState(new LandState(this, rb), StateLayer.Movement);
            // Action Layer
            stateMachine.AddState(new NoActionState(this, rb), StateLayer.Action);
            stateMachine.AddState(new CruiseState(this, rb), StateLayer.Action);
            stateMachine.AddState(new PushState(this, rb), StateLayer.Action);
            stateMachine.AddState(new PowerGrindState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickAState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickBState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickBBoostState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickCState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickCBoostState(this, rb), StateLayer.Action);
            stateMachine.AddState(new TrickDState(this, rb), StateLayer.Action);
            stateMachine.AddState(new GrindState(this, rb), StateLayer.Action);
            stateMachine.AddState(new WallRideState(this, rb), StateLayer.Action);
            stateMachine.AddState(new RecoveryState(this, rb), StateLayer.Action);
            stateMachine.AddState(new HitState(this, rb), StateLayer.Action);
            // 初始各层状态
            stateMachine.SwitchState<IdleState>(StateLayer.Movement);
            stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        }

        private void FixedUpdate()
        {
            playerSystem?.ApplyMovement();
            playerModel.VelocityLastFrame.Value = rb.linearVelocity;
            playerSystem?.ApplyRotation();
            if (playerModel.IsGrounded.Value)
            {
                collisionSystem.GroundSnap(bodyCollider, rb);
            }
        }

        protected override void OnRealTimeUpdate()
        {
            /// 删除射击相关代码
            // DetectInput();

            // HandleAimAndShoot();
            
            // 更新着地状态
            collisionSystem.GroundCheck(transform.position);

            // 检测前方墙壁(远距离)
            float wallCheckDistance = Mathf.Lerp(playerModel.Config.Value.wallCheckDistanceFarSlow,playerModel.Config.Value.wallCheckDistanceFarFast, 
                Mathf.Abs(rb.linearVelocity.x) / playerModel.Config.Value.maxPushSpeed);
            var (isNearWall, angle) = collisionSystem.WallCheck(bottomLeft.position, bottomRight.position, wallCheckDistance);
            playerModel.IsNearFgWall.Value = isNearWall;
            playerModel.FgWallAngle.Value = angle;

            // 检测前方墙壁(近距离), 用上一帧速度以防取到碰撞后速度
            var (touchingWall, wallAngle) = collisionSystem.WallCheck(bottomLeft.position, bottomRight.position, playerModel.Config.Value.wallCheckDistanceNear);
            playerModel.touchingWall.Value = touchingWall;

            // 检测玩家是否掉落过低
            if (CheckFallOutOfBounds())
            {
                // 重生过程中玩家可能被设为 inactive，避免继续执行并启动协程
                return;
            }

            // 更新当前State
            stateMachine.UpdateCurrentState();

            // 每 N 秒记录一次路径点（用于死亡后显示轨迹）
            if (traceSystem != null && tracePointPrefab != null)
            {
                traceTimer += Time.deltaTime;
                if (traceTimer >= TraceInterval)
                {
                    traceTimer = 0f;
                    traceSystem.AddPoint(transform.position);
                }
            }

            // 检测玩家方向变化并更新粒子特效
            CheckPlayerDirectionChange();

            //过几秒自动清空tricklist和grade
            if (this.GetSystem<ITrickSystem>().TrickList.Value.Count > 0
                && _clearTricksRoutine == null
                && isActiveAndEnabled
                && gameObject.activeInHierarchy)
            {
                _clearTricksRoutine = StartCoroutine(ClearTricksAfterDelay(5f));
            }
        }
        //过几秒自动清空tricklist和grade
        private IEnumerator ClearTricksAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            this.GetSystem<ITrickSystem>().RemoveAllTricks();
            this.GetModel<ITrickListModel>().Grade.Value = 'D';
            _clearTricksRoutine = null;
        }

        // 检测玩家是否掉落过低
        private bool CheckFallOutOfBounds()
        {
            if (transform.position.y < -20f)
            {
                
                var respawnSystem = this.GetSystem<IRespawnSystem>();
                respawnSystem.RespawnPlayer();
                return true;
            }

            return false;
        }

        // 提供给状态机使用的方法
        // public void DetectInput()
        // {
        //     SwitchItem();
        // }

        public Rigidbody2D GetRigidbody()
        {
            return rb;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_clearTricksRoutine != null)
            {
                StopCoroutine(_clearTricksRoutine);
                _clearTricksRoutine = null;
            }

            if (_slamHitboxRoutine != null)
            {
                StopCoroutine(_slamHitboxRoutine);
                _slamHitboxRoutine = null;
            }
        }
        
        #region Hitbox
        public void OpenSlamHitbox(float duration)
        {
            if (slamHitbox == null) return;
            if (_slamHitboxRoutine != null) StopCoroutine(_slamHitboxRoutine);
            slamHitbox.enabled = true;
            _slamHitboxRoutine = StartCoroutine(SlamHitboxRoutine(duration));
        }

        IEnumerator SlamHitboxRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (slamHitbox != null) slamHitbox.enabled = false;
            _slamHitboxRoutine = null;
        }
        #endregion


        /// <summary>
        /// 检测玩家是否进入滑轨或墙壁
        /// </summary>
        #region Collision
        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"OnTriggerEnter2D: {other.name} (isTrigger: {other.isTrigger})");

            if (other.isTrigger)
            {
                Track track = SafeGetComponent<Track>(other.gameObject);
                if (track != null)
                {
                    Debug.Log($"检测到滑轨: {track.name}");
                    playerModel.CurrentTrack.Value = track;
                    playerModel.IsNearTrack.Value = true;
                }

                Wall wall = other.GetComponent<Wall>();
                if (wall != null)
                {
                    Debug.Log($"检测到墙壁: {wall.name}");
                    playerModel.CurrentBgWall.Value = wall;
                    playerModel.IsNearBgWall.Value = true;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.isTrigger)
            {
                Track track = SafeGetComponent<Track>(other.gameObject);
                if (track != null && track == playerModel.CurrentTrack.Value)
                {
                    Debug.Log($"离开滑轨: {track.name}");
                    playerModel.IsNearTrack.Value = false;
                    playerModel.CurrentTrack.Value = null;
                }

                Wall wall = other.GetComponent<Wall>();
                if (wall != null)
                {
                    Debug.Log($"离开墙壁: {wall.name}");
                    playerModel.CurrentBgWall.Value = null;
                    playerModel.IsNearBgWall.Value = false;
                }
            }
        }
        #endregion

        #region Aim and Shoot
        // private void HandleAimAndShoot()
        // {
        //     if (disableInput) return;
        //     // 按住R键进入瞄准
        //     if (inputModel.ShootStart.Value)
        //     {
        //         playerModel.IsAiming.Value = true;
        //         playerModel.AimTimer.Value = 0f; // 重置计时器
        //         Time.timeScale = 0.2f;
        //         Time.fixedDeltaTime = 0.02f * Time.timeScale;
        //         if (aimLine != null) aimLine.enabled = true;
        //     }

        //     // 松开R键发射子弹
        //     if (inputModel.ShootEnd.Value)
        //     {
        //         if (playerModel.IsAiming.Value)
        //         {
        //             FireBullet();
        //         }
        //         StopAiming();
        //     }

        //     if (playerModel.IsAiming.Value)
        //     {
        //         // 更新瞄准计时器
        //         playerModel.AimTimer.Value += Time.unscaledDeltaTime; // 使用unscaledDeltaTime因为时间被放慢了

        //         // 检查是否超过最大瞄准时间
        //         if (playerModel.AimTimer.Value >= playerModel.MaxAimTime.Value)
        //         {
        //             StopAiming();
        //             return;
        //         }

        //         Vector2 direction = inputModel.AimDirection.Value;

        //         // 更新瞄准线
        //         if (aimLine != null)
        //         {
        //             aimLine.SetPosition(0, transform.position);
        //             aimLine.SetPosition(1, (Vector2)transform.position + direction * aimLineLength);
        //         }
        //     }
        // }

        // private void StopAiming()
        // {
        //     playerModel.IsAiming.Value = false;
        //     playerModel.AimTimer.Value = 0f;
        //     Time.timeScale = 1f;
        //     Time.fixedDeltaTime = 0.02f * Time.timeScale;
        //     if (aimLine != null) aimLine.enabled = false;
        // }

        // private void FireBullet()
        // {
        //     if (playerModel.Config.Value.bulletPrefabs.Length == 0) return;

        //     if(playerModel.CurrentBulletCount.Value <= 0) return;

        //     GameObject bulletPrefab = playerModel.Config.Value.bulletPrefabs[playerModel.CurrentBulletIndex.Value];
        //     if (bulletPrefab == null) return;

        //     Vector2 direction = inputModel.AimDirection.Value.normalized;

        //     GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        //     var ice = bullet.GetComponent<IceBullet>();
        //     if (ice != null)
        //     {
        //         ice.SetDirection(direction);
        //     }
        //     else
        //     {
        //         Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        //         if (bulletRb != null)
        //         {
        //             bulletRb.linearVelocity = direction * playerModel.Config.Value.bulletSpeed;
        //         }
        //     }
        //     playerModel.CurrentBulletCount.Value--;

        // }

        // // 获取当前瞄准计时器值（供UI使用）
        // public float GetAimTimer()
        // {
        //     return playerModel.AimTimer.Value;
        // }
        
        // // 重置瞄准时间上限到基础值
        // public void ResetAimTimeToBase()
        // {
        //     playerModel.MaxAimTime.Value = playerModel.Config.Value.baseMaxAimTime;
        //     Debug.Log($"InputController: 重置瞄准时间上限到基础值: {playerModel.MaxAimTime.Value}秒");
        // }
        
        // private void SwitchItem()
        // {
        //     if (inputModel.SwitchItem.Value)
        //     {
        //         playerModel.CurrentBulletIndex.Value =
        //         (playerModel.CurrentBulletIndex.Value + 1) % playerModel.Config.Value.bulletPrefabs.Length;
        //         Debug.Log("当前子弹类型: " + playerModel.CurrentBulletIndex.Value);
        //     }
        // }

        // // 处理落地时的瞄准时间奖励
        // public void HandleLandingAimTimeBonus()
        // {
        //     if (playerModel.HasPerformedTrickInAir.Value)
        //     {
        //         // 增加瞄准时间上限1秒
        //         playerModel.MaxAimTime.Value += 1f;
        //         Debug.Log($"InputController: 落地奖励！瞄准时间上限增加1秒，当前上限: {playerModel.MaxAimTime.Value}秒");

        //         // 重置标志
        //         playerModel.HasPerformedTrickInAir.Value = false;
        //     }
        // }


        // // 获取基础瞄准时间上限（供UI使用）
        // public float baseMaxAimTime => playerModel.Config.Value.baseMaxAimTime;

        #endregion

        #region Player Direction
        // 检测玩家方向并更新例子特效容器
        private void CheckPlayerDirectionChange()
        {
            // 检测是否有移动输入
            if (Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            {
                bool shouldFaceRight = rb.linearVelocity.x > 0;
                // 检测方向是否改变
                if (shouldFaceRight != playerModel.IsFacingRight.Value)
                {
                    // 方向改变，粒子特效容器旋转180度
                    if (particleEffectContainer != null)
                    {
                        Vector3 currentRotation = particleEffectContainer.localEulerAngles;
                        particleEffectContainer.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180f, currentRotation.z);
                        UpdatePlayerDirection(shouldFaceRight);
                    }
                }
            }
            // 更新Sprite方向
            sprite.transform.localScale = new Vector3(playerModel.IsFacingRight.Value ? 1 : -1, 1, 1);
        }

        public void UpdatePlayerDirection(bool isFacingRight)
        {
            playerModel.IsFacingRight.Value = isFacingRight;
            sprite.transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        }
        #endregion

        #region Death Trace
        /// <summary>
        /// 绘制死亡路径点（由 TraceSystem 在玩家死亡时调用）
        /// </summary>
        /// <param name="points">路径上的点，用 tracePointPrefab 画</param>
        /// <param name="deathPos">死亡瞬间位置，用 traceDeathPointPrefab 画（若未设则用 tracePointPrefab）</param>
        public void DrawDeathTracePoints(List<Vector2> points, Vector2? deathPos = null)
        {
            bool hasPathPrefab = tracePointPrefab != null;
            GameObject deathPrefab = traceDeathPointPrefab != null ? traceDeathPointPrefab : tracePointPrefab;
            bool hasDeath = deathPos.HasValue && deathPrefab != null;
            if (!hasPathPrefab && !hasDeath) return;

            ClearSpawnedTracePoints();

            if (hasPathPrefab && points != null && points.Count > 0)
            {
                foreach (var pos in points)
                    SpawnOneTracePoint(pos, tracePointPrefab);
            }
            if (hasDeath)
                SpawnOneTracePoint(deathPos.Value, deathPrefab);
        }

        private void SpawnOneTracePoint(Vector2 pos, GameObject prefab)
        {
            var p = Instantiate(prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 100;
            spawnedTracePoints.Add(p);
        }

        private void ClearSpawnedTracePoints()
        {
            foreach (var go in spawnedTracePoints)
            {
                if (go != null) Destroy(go);
            }
            spawnedTracePoints.Clear();
        }
        #endregion

        #region Helper Methods
        public T SafeGetComponent<T>(GameObject obj) where T : MonoBehaviour
        {
            T get1 = obj.GetComponent<T>();
            CodeReferencer get2 = obj.GetComponent<CodeReferencer>();

            if (get1 != null)
            {
                return get1;

            }
            else if (get2 != null && (get2.reference is T))
            {
                return (get2.reference as T);

             }

            return null;
        }
        

        // // 标记已执行trick（由TrickState调用）
        public void MarkTrickPerformed()
        {
            playerModel.HasPerformedTrickInAir.Value = true;
            Debug.Log("InputController: 标记已执行trick");
        }
        #endregion
    }
}