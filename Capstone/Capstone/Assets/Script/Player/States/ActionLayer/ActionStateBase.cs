using Unity.VisualScripting;
using UnityEngine;
using SkateGame;

// Base class for action-layer states that may suppress movement
public abstract class ActionStateBase : StateBase
{
    protected float stateTimer;
    protected float stateTotalDuration = -1f;
    protected bool isLoop = true;
    protected Vector2 ignoreMovementLayerDuration = new Vector2(-1f, -1f); // 第一个参数是开始忽略的时间，第二个参数是结束忽略的时间
    protected Vector2 recoveryDuration = new Vector2(-1f, -1f); // 第一个参数是开始后摇的时间，第二个参数是结束后摇的时间
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
        CheckIgnoreMovementLayer();
        CheckRecovering();
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
        StateTimeUpdate();
        CheckIgnoreMovementLayer();
        CheckRecovering();

        if(playerModel.IsRecovering.Value)
        {
            CheckSwitchAction();
        }
        UpdateActionState();
    }
    public sealed override void Exit()
    {
        ExitActionState();
    }

    private void StateTimeUpdate()
    {
        stateTimer += Time.deltaTime;
        if(!isLoop && stateTimer > stateTotalDuration)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
        }
    }
    
    // 检查是否忽略运动层
    private void CheckIgnoreMovementLayer()
    {
        if(isLoop)
        {
            playerModel.IsIgnoringMovementLayer.Value = ignoreMovementLayerDuration.x == -1f ? false : true;
        }
        else
        {      
            if(stateTimer > ignoreMovementLayerDuration.x && stateTimer < ignoreMovementLayerDuration.y)
            {
                playerModel.IsIgnoringMovementLayer.Value = true;
            }
            else playerModel.IsIgnoringMovementLayer.Value = false;
        }
    }

    // 检查是否后摇
    private void CheckRecovering()
    {
        if(isLoop) playerModel.IsRecovering.Value = true;
        else
        {
            if(stateTimer > recoveryDuration.x && stateTimer < recoveryDuration.y)
            {
                playerModel.IsRecovering.Value = true;
            }
            else playerModel.IsRecovering.Value = false;
        }
    }

    // 检查是否切换到其他状态
    private void CheckSwitchAction()
    {
        /*
        state再增多的话顺序可能需要调整
        */
        // 优先Trick
        if(inputModel.TrickStart.Value && !playerModel.IsGrounded.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "TrickA");
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

        // 循环且无输入时None
        else if (isLoop)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
        }
    }
    private void GrindInput()
    {
        // 优先滑轨
        if (playerModel.GrindJumpTimer.Value <= 0f && playerModel.IsNearTrack.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "Grind");
        }
        // 其次滑墙
        else if (!playerModel.IsGrounded.Value)
        {
            if(playerModel.WallRideCooldownTimer.Value <= 0f && playerModel.IsNearWall.Value)
            {
                player.stateMachine.SwitchState(StateLayer.Action, "WallRide");
            }
            // 最后Grab
            else
            {
                player.stateMachine.SwitchState(StateLayer.Action, "Grab");
            }
        }
    }
}