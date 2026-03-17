using SkateGame;
using QFramework;
using UnityEngine;
public abstract class GroundMovementState : StateBase, ICanGetSystem
{
    protected bool WasGrounded => this.GetModel<IPlayerModel>().WasGrounded.Value;
    protected bool IsGrounded => this.GetModel<IPlayerModel>().IsGrounded.Value;
    protected virtual void UpdateGroundMovement() { }
    protected virtual void EnterGroundMovement() { }
    protected virtual void ExitGroundMovement() { }

    public sealed override void Update()
    {
        float moveInput = inputModel.Move.Value.x;
        player.SendEvent<MoveInputEvent>(new MoveInputEvent { HorizontalInput = moveInput });
        UpdateGroundMovement();
        switchAirborneMovement();
        CheckCrash();
    }

    public sealed override void Enter()
    {
        EnterGroundMovement();
        playMoving();
    }

    public sealed override void Exit()
    {
        ExitGroundMovement();
        pauseMoving();
    }

    private void CheckCrash()
    {
        if (!this.GetSystem<ICollisionSystem>().CheckCrash(playerModel.VelocityLastFrame.Value, playerModel.FgWallAngle.Value))
            return;
        Vector2 n = ((Vector2)(Quaternion.Euler(0f, 0f, playerModel.FgWallAngle.Value) * Vector2.up)).normalized;
        playerModel.HitKnockbackDirection.Value = n;
        playerModel.WallJumpWallNormal.Value = n;
        player.stateMachine.SwitchState<HitState>(StateLayer.Action);
    }

    private void switchAirborneMovement()
    {
        if (inputModel.JumpStart.Value && !playerModel.IsIgnoringMovementLayer.Value)
            player.stateMachine.SwitchState<JumpState>(StateLayer.Movement);
        else CheckFall();
    }
    private void CheckFall()
    {
        if (WasGrounded && !IsGrounded)
        {
            playerModel.CoyoteTimer.Value = playerModel.Config.Value.coyoteTime;
            player.stateMachine.SwitchState<AirState>(StateLayer.Movement);
        }
    }

    public void playMoving()
    {
        // FOR JERRY'S AUDIO - SKATEBOARD MOVING
        AudioManager.Instance.fmodPlayMove();
    }
    public void pauseMoving()
    {
        if(AudioManager.Instance != null)
        {
            AudioManager.Instance.fmodPauseMove();
        }
    }
}
