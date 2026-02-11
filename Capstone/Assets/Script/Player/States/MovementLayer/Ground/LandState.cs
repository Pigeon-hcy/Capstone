using UnityEngine;
using SkateGame;
using QFramework;

public class LandState : GroundMovementState, ICanSendEvent
{
    private float landTimer;
    private bool isDoubleJumpLand;



    public LandState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        playerModel.CurrentBulletCount.Value = playerModel.Config.Value.bulletMaxCount;
        isDoubleJumpLand = playerModel.CanDoubleJump.Value;
        playerModel.CanDoubleJump.Value = true;
        landTimer = 0f;
        player.SendEvent<PlayerLandedEvent>();
        playLanding();
        if (player.landEffectPlayer != null)
        {
            player.landEffectPlayer.PlayFeedbacks();
        }
    }

    protected override void UpdateGroundMovement()
    {
        UpdateLandTimer();
    }

    protected override void ExitGroundMovement()
    {
    }

    private void UpdateLandTimer()
    {
        if(inputModel.Push.Value)
        {
            player.stateMachine.SwitchState<PushState>(StateLayer.Movement);
        }
        // change land duration based on jump type
        // TODO: no double jump anymore
        if(isDoubleJumpLand && landTimer < playerModel.LandDuration.Value ||
            !isDoubleJumpLand && landTimer < playerModel.DoubleJumpLandDuration.Value)
        {
            landTimer += Time.deltaTime;
        }
        else
        {
            if(rb.linearVelocity.x == 0)
            {
                player.stateMachine.SwitchState<IdleState>(StateLayer.Movement);
            }
            else
            {
                player.stateMachine.SwitchState<MoveState>(StateLayer.Movement);
                
            }
        }
    }
    public void playLanding()
    {
        AudioManager.Instance.fmodPlayLanding();
    }
} 