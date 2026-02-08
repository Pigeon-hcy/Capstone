using SkateGame;
using QFramework;
public abstract class GroundMovementState : StateBase
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

    private void switchAirborneMovement()
    {
        if (inputModel.JumpStart.Value && !playerModel.IsIgnoringMovementLayer.Value)
        {
            player.stateMachine.SwitchState<JumpState>(StateLayer.Movement);
        }
        else
        {
            CheckFall();
        }
    }
    private void CheckFall()
    {
        if (WasGrounded && !IsGrounded)
        {
            player.stateMachine.SwitchState<AirState>(StateLayer.Movement);
        }
    }

    public void playMoving()
    {
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
