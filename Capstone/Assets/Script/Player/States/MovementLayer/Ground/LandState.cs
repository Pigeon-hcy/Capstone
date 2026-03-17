using UnityEngine;
using SkateGame;
using QFramework;

public class LandState : GroundMovementState, ICanSendEvent
{
    private float landTimer;

    public LandState(PlayerController player, Rigidbody2D rb)
    {
        this.player = player;
        this.rb = rb;
    }

    protected override void EnterGroundMovement()
    {
        playerModel.CurrentBulletCount.Value = playerModel.Config.Value.bulletMaxCount;
        playerModel.CoyoteTimer.Value = 0f;
        landTimer = 0f;
        player.SendEvent<PlayerLandedEvent>();
        Vector2 groundUp = (Quaternion.Euler(0f, 0f, playerModel.TargetRotationDeg.Value) * Vector2.up).normalized;
        float impactSpeed = -Vector2.Dot(playerModel.VelocityLastFrame.Value, groundUp);
        
        if (impactSpeed > playerModel.Config.Value.landingSpeedThreshold)
        {
            // FOR JERRY'S AUDIO - LANDING
            playLanding();
            if (player.landEffectPlayer != null)
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
        // change land duration based on jump type
        // TODO: no double jump anymore
        if(landTimer < playerModel.LandDuration.Value ||
            !playerModel.IsGrindJump.Value && landTimer < playerModel.DoubleJumpLandDuration.Value)
        {
            landTimer += Time.deltaTime;
        }
        else
        {
            if(rb.linearVelocity.x < 0.01f)
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