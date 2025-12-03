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
        if(GetStateName() != "None")
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
        if(!isLoop && stateTimer > stateDuration && GetStateName() != "Recovery")
        {
            player.stateMachine.SwitchState(StateLayer.Action, "Recovery");
        }
    }
    public sealed override void Exit()
    {
        ExitActionState();
    }

    protected void CheckSwitchAction()
    {
        // 优先Trick
        if(inputModel.TrickAStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "TrickA");
        }
        else if(inputModel.TrickBStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "TrickB");
        }
        else if(inputModel.TrickCStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "TrickC");
        }
        else if (inputModel.Push.Value && playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "Push");
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
            player.stateMachine.SwitchState(StateLayer.Action, "Grind");
        }
        // 其次滑墙
        else if (!playerModel.IsGrounded.Value)
        {
            if(playerModel.WallRideCooldownTimer.Value <= 0f && playerModel.IsNearBgWall.Value)
            {
                player.stateMachine.SwitchState(StateLayer.Action, "WallRide");
            }
        }   
    }
}