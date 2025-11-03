using UnityEngine;
using QFramework;
using System.Collections;
using MoreMountains.Feedbacks;
using System;

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
        private IInputModel inputModel;
        [Header("状态机")]
        public LayeredStateMachine stateMachine;
        private Rigidbody2D rb;

        [Header("Animation")]
        public Animator animator;

        [Header("瞄准与射击设置")]
        public LineRenderer aimLine;      // 瞄准线 temp
        public float aimLineLength = 10f; // 瞄准线长度

        [Header("MMF效果")]
        public MMF_Player moveEffect;
        public MMF_Player powerGrindEffect;
        public MMF_Player ReverseEffect;
        public MMF_Player GrindEffect;
        public MMF_Player WallRideEffect;
        public MMF_Player TrickAEffect;
        public MMF_Player TrickBEffect;
        public MMF_Player TrickABoostEffect;
        public MMF_Player AirEffect;
        public MMF_Player JumpEffect;
        public MMF_Player GJumpEffect;
        public MMF_Player DoubleJumpEffect;
        public MMF_Player GrabEffect;
        public MMF_Player IdleEffect;
        public MMF_Player powerGrindEffectPlayer;

        [Header("粒子特效容器")]
        public Transform particleEffectContainer; // 粒子特效容器
        protected override void InitializeController()
        {
            // 获取玩家参数
            playerModel = this.GetModel<IPlayerModel>();
            inputModel = this.GetModel<IInputModel>();
            this.GetSystem<IPlayerAssetSystem>().SetPlayerConfig(playerConfig);

            // 获取组件
            rb = GetComponent<Rigidbody2D>();

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
            stateMachine.AddState("Idle", new IdleState(this, rb), StateLayer.Movement);
            stateMachine.AddState("Jump", new JumpState(this, rb), StateLayer.Movement);
            stateMachine.AddState("Move", new MoveState(this, rb), StateLayer.Movement);
            stateMachine.AddState("Air", new AirState(this, rb), StateLayer.Movement);
            stateMachine.AddState("DoubleJump", new DoubleJumpState(this, rb), StateLayer.Movement);
            stateMachine.AddState("PowerGrind", new PowerGrindState(this, rb), StateLayer.Movement);
            stateMachine.AddState("Reverse", new ReverseState(this, rb), StateLayer.Movement);
            stateMachine.AddState("Land", new LandState(this, rb), StateLayer.Movement);
            // Action Layer
            stateMachine.AddState("None", new NoActionState(this, rb), StateLayer.Action);
            stateMachine.AddState("TrickA", new TrickAState(this, rb), StateLayer.Action);
            stateMachine.AddState("Grind", new GrindState(this, rb), StateLayer.Action);
            stateMachine.AddState("Grab", new GrabbingState(this, rb), StateLayer.Action);
            stateMachine.AddState("WallRide", new WallRideState(this, rb), StateLayer.Action);
            stateMachine.AddState("Push", new PushState(this, rb), StateLayer.Action);
            // 初始各层状态
            stateMachine.SwitchState(StateLayer.Movement, "Idle");
            stateMachine.SwitchState(StateLayer.Action, "None");
        }

        protected override void OnRealTimeUpdate()
        {
            // 检测输入并发送事件
            DetectInput();

            HandleAimAndShoot();
            
            // 更新着地状态
            IsGrounded();

            // 更新冷却计时器
            UpdateCooldownTimers();

            // 检测玩家是否掉落过低
            CheckFallOutOfBounds();

            // 更新当前State
            stateMachine.UpdateCurrentState();

            // 检测玩家方向变化并更新粒子特效
            CheckPlayerDirectionChange();

            //过几秒自动清空tricklist和grade
            if (this.GetSystem<ITrickSystem>().TrickList.Value.Count > 0)
            {
                StartCoroutine(ClearTricksAfterDelay(5f));
            }
        }
        //过几秒自动清空tricklist和grade
        private IEnumerator ClearTricksAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            this.GetSystem<ITrickSystem>().RemoveAllTricks();
            this.GetModel<ITrickListModel>().Grade.Value = 'D';
        }

        // 更新各种冷却计时器
        private void UpdateCooldownTimers()
        {
            // 更新滑墙冷却计时器
            if (playerModel.WallRideCooldownTimer.Value > 0f)
            {
                playerModel.WallRideCooldownTimer.Value -= Time.deltaTime;
                if (playerModel.WallRideCooldownTimer.Value < 0f)
                {
                    playerModel.WallRideCooldownTimer.Value = 0f;
                }
            }
        }

        // 检测玩家是否掉落过低
        private void CheckFallOutOfBounds()
        {
            if (transform.position.y < -20f)
            {
                
                var respawnSystem = this.GetSystem<IRespawnSystem>();
                respawnSystem.RespawnPlayer();
                
            }
        }

        // 提供给状态机使用的方法
        public void DetectInput()
        {
            SwitchItem();
            Trick();
        }
        private void SwitchItem()
        {
            if (inputModel.SwitchItem.Value)
            {
                playerModel.CurrentBulletIndex.Value =
                (playerModel.CurrentBulletIndex.Value + 1) % playerModel.Config.Value.bulletPrefabs.Length;
                Debug.Log("当前子弹类型: " + playerModel.CurrentBulletIndex.Value);
            }
        }
        private void Trick()
        {
            if (inputModel.TrickStart.Value && stateMachine.GetMovementStateName() == "Air")
            {
                this.SendEvent<TrickAInputEvent>();
            }
        }

        #region Collision
        private bool IsGrounded()
        {
            // 使用多个射线检测来提高准确性
            Vector2 rayStart = transform.position;
            Vector2 rayDirection = Vector2.down;
            float rayDistance = 0.35f; // 减少检测距离，避免误判

            // 主射线检测
            RaycastHit2D hit = Physics2D.Raycast(rayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);

            // 如果主射线没检测到，尝试左右偏移的射线
            if (hit.collider == null)
            {
                Vector2 leftRayStart = rayStart + Vector2.left * 0.3f;
                Vector2 rightRayStart = rayStart + Vector2.right * 0.3f;

                RaycastHit2D leftHit = Physics2D.Raycast(leftRayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);
                RaycastHit2D rightHit = Physics2D.Raycast(rightRayStart, rayDirection, rayDistance, playerModel.Config.Value.groundLayer);

                if (leftHit.collider != null)
                {
                    hit = leftHit;
                }
                else if (rightHit.collider != null)
                {
                    hit = rightHit;
                }
            }

            bool grounded = hit.collider != null;
            playerModel.WasGrounded.Value = playerModel.IsGrounded.Value;
            playerModel.IsGrounded.Value = grounded;
            return grounded;
        }
        #endregion

        public Rigidbody2D GetRigidbody()
        {
            return rb;
        }

        // 奖励跳跃方法 - 用于高等级技巧奖励
        public void RewardJump()
        {
            if (rb != null)
            {
                // 给予一个额外的跳跃力
                Vector2 currentVelocity = rb.linearVelocity;
                rb.linearVelocity = new Vector2(currentVelocity.x, 10f);
                Debug.Log("奖励跳跃！获得额外跳跃力");
            }
        }

        // 延迟切换状态的协程
        public IEnumerator SwitchToStateDelayed(string stateName)
        {
            yield return new WaitForSeconds(0.1f);
            stateMachine.SwitchState(StateLayer.Action, stateName);
        }

        // 碰撞检测
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
                    playerModel.CurrentWall.Value = wall;
                    playerModel.IsNearWall.Value = true;
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
                    playerModel.CurrentWall.Value = null;
                    playerModel.IsNearWall.Value = false;
                }
            }
        }

        private void HandleAimAndShoot()
        {
            // 按住R键进入瞄准
            if (inputModel.ShootStart.Value)
            {
                playerModel.IsAiming.Value = true;
                playerModel.AimTimer.Value = 0f; // 重置计时器
                Time.timeScale = 0.2f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                if (aimLine != null) aimLine.enabled = true;
            }

            // 松开R键发射子弹
            if (inputModel.ShootEnd.Value)
            {
                if (playerModel.IsAiming.Value)
                {
                    FireBullet();
                }
                StopAiming();
            }

            if (playerModel.IsAiming.Value)
            {
                // 更新瞄准计时器
                playerModel.AimTimer.Value += Time.unscaledDeltaTime; // 使用unscaledDeltaTime因为时间被放慢了

                // 检查是否超过最大瞄准时间
                if (playerModel.AimTimer.Value >= playerModel.MaxAimTime.Value)
                {
                    StopAiming();
                    return;
                }

                Vector2 direction = inputModel.AimDirection.Value;

                // 更新瞄准线
                if (aimLine != null)
                {
                    aimLine.SetPosition(0, transform.position);
                    aimLine.SetPosition(1, (Vector2)transform.position + direction * aimLineLength);
                }
            }
        }

        private void StopAiming()
        {
            playerModel.IsAiming.Value = false;
            playerModel.AimTimer.Value = 0f;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            if (aimLine != null) aimLine.enabled = false;
        }

        private void FireBullet()
        {
            if (playerModel.Config.Value.bulletPrefabs.Length == 0) return;

            if(playerModel.CurrentBulletCount.Value <= 0) return;

            GameObject bulletPrefab = playerModel.Config.Value.bulletPrefabs[playerModel.CurrentBulletIndex.Value];
            if (bulletPrefab == null) return;

            Vector2 direction = inputModel.AimDirection.Value.normalized;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            var ice = bullet.GetComponent<IceBullet>();
            if (ice != null)
            {
                ice.SetDirection(direction);
            }
            else
            {
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = direction * playerModel.Config.Value.bulletSpeed;
                }
            }
            playerModel.CurrentBulletCount.Value--;

        }

        // 获取当前瞄准计时器值（供UI使用）
        public float GetAimTimer()
        {
            return playerModel.AimTimer.Value;
        }

        // 标记已执行trick（由TrickState调用）
        public void MarkTrickPerformed()
        {
            playerModel.HasPerformedTrickInAir.Value = true;
            Debug.Log("InputController: 标记已执行trick");
        }

        // 处理落地时的瞄准时间奖励
        public void HandleLandingAimTimeBonus()
        {
            if (playerModel.HasPerformedTrickInAir.Value)
            {
                // 增加瞄准时间上限1秒
                playerModel.MaxAimTime.Value += 1f;
                Debug.Log($"InputController: 落地奖励！瞄准时间上限增加1秒，当前上限: {playerModel.MaxAimTime.Value}秒");

                // 重置标志
                playerModel.HasPerformedTrickInAir.Value = false;
            }
        }

        // 重置瞄准时间上限到基础值
        public void ResetAimTimeToBase()
        {
            playerModel.MaxAimTime.Value = playerModel.Config.Value.baseMaxAimTime;
            Debug.Log($"InputController: 重置瞄准时间上限到基础值: {playerModel.MaxAimTime.Value}秒");
        }

        // 获取基础瞄准时间上限（供UI使用）
        public float baseMaxAimTime => playerModel.Config.Value.baseMaxAimTime;

        // 检测玩家方向并更新例子特效容器
        private void CheckPlayerDirectionChange()
        {
            float currentMoveInput = inputModel.Move.Value.x;

            // 检测是否有移动输入
            if (Mathf.Abs(currentMoveInput) > 0.01f)
            {
                bool shouldFaceRight = currentMoveInput > 0;
                // 检测方向是否改变
                if (shouldFaceRight != playerModel.IsFacingRight.Value)
                {
                    // 方向改变，粒子特效容器旋转180度
                    if (particleEffectContainer != null)
                    {
                        Vector3 currentRotation = particleEffectContainer.localEulerAngles;
                        particleEffectContainer.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180f, currentRotation.z);
                        playerModel.IsFacingRight.Value = shouldFaceRight;
                    }
                }
            }
        }

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
    }
}