using Unity.VisualScripting;
using UnityEngine;
using SkateGame;

// Base class for action-layer states that may suppress movement
public abstract class ActionStateBase : StateBase
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
        // 优先Trick
        if(inputModel.GrabStart.Value)
        {
            player.stateMachine.SwitchState<TrickDState>(StateLayer.Action);
        }
        else if(inputModel.DashStart.Value)
        {
            player.stateMachine.SwitchState<TrickBState>(StateLayer.Action);
        }
        // TODO: Design Slam On ground
        else if(inputModel.SlamStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<TrickCState>(StateLayer.Action);
        }
        else if (inputModel.PushStart.Value && playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState<PushState>(StateLayer.Action);
        }
        // 其次Grind
        else if (inputModel.Grind.Value)
        {
            GrindInput();
        }
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
}