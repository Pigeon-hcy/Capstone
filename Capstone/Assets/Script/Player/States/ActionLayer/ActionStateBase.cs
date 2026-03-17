using QFramework;
using UnityEngine;
using SkateGame;

// Base class for action-layer states that may suppress movement
public abstract class ActionStateBase : StateBase, ICanGetSystem, IBelongToArchitecture, ICanRegisterEvent
{
    protected float stateTimer;
    protected float stateDuration = -1f;
    protected bool isLoop = true;
    protected bool ignoringMovementLayer = false;

    protected virtual void UpdateActionState(){}
    protected virtual void EnterActionState(){}
    protected virtual void ExitActionState(){}
    protected ActionStateBase(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
        this.RegisterEvent<WallHitEvent>(OnWallHit);
    }
    public sealed override void Enter()
    {
        stateTimer = 0f;
        playerModel.IsIgnoringMovementLayer.Value = ignoringMovementLayer;
        
        /* 
        设置动画层权重 
        */
        if(!(this is NoActionState))
        {
            player.animator.SetLayerWeight(0, 0);
            player.animator.SetLayerWeight(1, 1);
        }
        else
        {
            player.animator.SetLayerWeight(0, 1);
            player.animator.SetLayerWeight(1, 0);}

        EnterActionState();
    }
    public sealed override void Update()
    {
        UpdateCooldownTimers();
        stateTimer += Time.deltaTime;
        UpdateActionState();
        
        // 非循环状态到期，切换到Recovery状态
        if(!isLoop && stateTimer > stateDuration && !(this is RecoveryState))
        {
            player.stateMachine.SwitchState<RecoveryState>(StateLayer.Action);
        }
    }
    public sealed override void Exit()
    {
        ExitActionState();
    }

    protected void CheckSwitchAction()
    {
        var energySystem = this.GetSystem<IEnergySystem>();
        // 优先Trick
        if(inputModel.GrabStart.Value && energySystem.HasEnoughEnergy(1))
        {
            energySystem.ConsumeEnergy(1);
            player.stateMachine.SwitchState<TrickDState>(StateLayer.Action);
        }
        else if(inputModel.DashStart.Value && energySystem.HasEnoughEnergy(1))
        {
            energySystem.ConsumeEnergy(1);
            player.stateMachine.SwitchState<TrickBState>(StateLayer.Action);
        }
        else if(inputModel.BrakeStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<TrickCState>(StateLayer.Action);
        }
        // 其次Grind
        else if (inputModel.Grind.Value)
        {
            GrindInput();
        }
    }

    protected void CheckPushAndPowerGrind()
    {
        if (inputModel.Push.Value && playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<PushState>(StateLayer.Action);
        }
        else if (!inputModel.Push.Value && playerModel.IsGrounded.Value && Mathf.Abs(playerModel.PushSpeed.Value) > playerModel.Config.Value.powerGrindStopSpeedThreshold)
        {
            player.stateMachine.SwitchState<PowerGrindState>(StateLayer.Action);
        }
    }
    
    protected float VRight()
    {
        Vector2 groundRight = (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.right).normalized;
        return Mathf.Abs(Vector2.Dot(rb.linearVelocity, groundRight));
    }
    protected void GrindInput()
    {
        // 优先滑轨
        if (playerModel.GrindJumpTimer.Value <= 0f && playerModel.IsNearTrack.Value)
        {
            player.stateMachine.SwitchState<GrindState>(StateLayer.Action);
        }
        // 其次滑墙
        else if (!playerModel.IsGrounded.Value)
        {
            if(playerModel.WallRideCooldownTimer.Value <= 0f && playerModel.IsNearBgWall.Value)
            {
                player.stateMachine.SwitchState<WallRideState>(StateLayer.Action);
            }
        }   
    }
    private void OnWallHit(WallHitEvent evt)
    {
        // Vector2 n = evt.wallNormal.normalized;
        // playerModel.HitKnockbackDirection.Value = n;
        // playerModel.WallJumpWallNormal.Value = n;
        // player.stateMachine.SwitchState<HitState>(StateLayer.Action);
    }

    private void UpdateCooldownTimers()
    {
        if (playerModel.WallRideCooldownTimer.Value > 0f)
        {
            playerModel.WallRideCooldownTimer.Value -= Time.deltaTime;
            if (playerModel.WallRideCooldownTimer.Value < 0f)
                playerModel.WallRideCooldownTimer.Value = 0f;
        }
        if (playerModel.ReversePushCooldownTimer.Value > 0f)
        {
            playerModel.ReversePushCooldownTimer.Value -= Time.deltaTime;
            if (playerModel.ReversePushCooldownTimer.Value < 0f)
                playerModel.ReversePushCooldownTimer.Value = 0f;
        }
    }
}