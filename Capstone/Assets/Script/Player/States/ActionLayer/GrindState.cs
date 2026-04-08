using UnityEngine;
using SkateGame;
using QFramework;
using Lofelt.NiceVibrations;

public class GrindState : ActionStateBase
{   
    private Vector2 direction;
    private float normalG;

    private TrackDirComputeTool trackRef;

    private int leftToRight = 1;
    /// <summary>进入滑轨时选中的 CM 偏转方向；退出时配对 Exit。</summary>
    private bool grindCameraIsLeftToRight;

    public GrindState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        normalG = playerModel.Config.Value.normalG;
        isLoop = playerModel.Config.Value.isLoopGrind;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerGrind;
        trackRef = null;
    }

    protected override void EnterActionState()
    {
        // 检查currentTrack是否为null
        if (playerModel.CurrentTrack.Value == null)
        {
            Debug.LogError("GrindState.Enter: currentTrack为null，无法进入滑轨状态");
            GrindJump();
            return;
        }
        
        Vector2 velocity = rb.linearVelocity;
        direction = new Vector2(velocity.x, 0).normalized;
        leftToRight = velocity.x >= 0f ? 1 : -1;
        playerModel.CurrentGravityScale.Value = 0f;
        playerModel.GrindDirection.Value = direction;
        trackRef = playerModel.CurrentTrack.Value.GetDirTool();
        SnapPlayerToTrack();
        
        if (player.GrindEffect != null)
        {
            player.GrindEffect.PlayFeedbacks();
        }
        if (player.cmControls != null && player.cmControls.Length > 0)
        {
            grindCameraIsLeftToRight = playerModel.IsFacingRight.Value;
            foreach (var cmControl in player.cmControls)
            {
                if (grindCameraIsLeftToRight)
                    cmControl.EnterSpecialCameraPositionWhenGrindLeftToRight();
                else
                    cmControl.EnterSpecialCameraPositionWhenGrindRightToLeft();
            }
        }
        // FOR JERRY'S AUDIO - RAIL GRIND
        playRailGrind();
        player.SendEvent<GrindInputEvent>(new GrindInputEvent { IsGrinding = true });
    }

    protected override void UpdateActionState()
    {
        // 首先检查E键是否按住，如果没有按住立即退出
        if (!inputModel.Grind.Value)
        {
            GrindJump();
            return;
        }
        
        // 如果按下跳跃键，直接跳跃
        if (inputModel.JumpStart.Value)
        {
            GrindJump();
            return;
        }
        // 然后检查currentTrack是否为null
        if (playerModel.CurrentTrack.Value == null)
        {
            GrindJump();
            return;
        }

        // Proactive end-of-track detection: trigger exit collider may not align with curve endpoint
        if (stateTimer > 0.1f)
        {
            float exitDist = playerModel.Config.Value.grindSpeed * Time.fixedDeltaTime * 3f;
            if ((player.transform.position - trackRef.GetExitPoint(leftToRight)).sqrMagnitude < exitDist * exitDist)
            {
                GrindJump();
                return;
            }
        }

        // 再次检查currentTrack，确保安全
        if (playerModel.CurrentTrack.Value != null)
        {
            

            //Vector2 moveDelta = direction * speed * Time.deltaTime;
            Vector3 pos = player.transform.position;

            Vector3 recordDir = direction;
            //新，更新direction
            direction = trackRef.GetNearestPointAndTangent(player.transform.position, leftToRight, player.transform.position,hintTangent: recordDir).tangent;
            Vector2 movement = direction * playerModel.Config.Value.maxMoveSpeed * Time.deltaTime;
            Vector3 recorcPos = player.transform.position;
            //Debug.Log("Direction的x是正的1"+ (direction.x>0));
            // player.transform.position = player.transform.position + new Vector3(movement.x,movement.y, 0);
            pos = trackRef.GetNearestPointAndTangent(player.transform.position, leftToRight, recorcPos, hintTangent: recordDir).nearest;


            //pos.x += moveDelta.x;
            //pos.y = playerModel.CurrentTrack.Value.GetTrackPosition().y+0.2f;
            // player.transform.position = pos;
            playerModel.GrindDirection.Value = direction;
        }
    }

    private void SnapPlayerToTrack()
    {
        if (playerModel.CurrentTrack.Value != null)
        {
            Vector3 trackPos = playerModel.CurrentTrack.Value.GetTrackPosition();
            Vector3 playerPos =  trackRef.GetNearestPointAndTangent(player.transform.position, leftToRight).nearest;
            //Debug.LogError("玩家现在位置是"+player.transform.position+", 玩家吸附的位置是"+ playerPos);
            //playerPos.y = trackPos.y+0.2f; 
            player.transform.position = playerPos;
        }
    }

    protected override void ExitActionState()
    {
        playerModel.CurrentGravityScale.Value = normalG;

        if (player.GrindEffect != null)
        {
            player.GrindEffect.StopFeedbacks();
        }
        if (player.cmControls != null && player.cmControls.Length > 0)
        {
            foreach (var cmControl in player.cmControls)
            {
                if (grindCameraIsLeftToRight)
                    cmControl.ExitSpecialCameraPositionWhenGrindLeftToRight();
                else
                    cmControl.ExitSpecialCameraPositionWhenGrindRightToLeft();
            }
        }
        // FOR JERRY'S AUDIO - RAIL GRIND STOP
        pauseRailGrind();
        player.SendEvent<GrindInputEvent>(new GrindInputEvent { IsGrinding = false });
    }

    private void playRailGrind()
    {
        AudioManager.Instance.fmodPlayRailGrind();
            // FOR JERRY'S AUDIO - LANDING
            AudioManager.Instance.fmodPlayLanding();
        HapticPatterns.PlayConstant(0.1f, 0.1f, 10f);
    }
    private void pauseRailGrind()
    {
        AudioManager.Instance.fmodPauseRailGrind();
        HapticController.Stop();
    }
    private void GrindJump()
    {
        playerModel.IsGrindJump.Value = true;
        player.stateMachine.SwitchState<NoActionState>(StateLayer.Action);
        player.stateMachine.SwitchState<JumpState>(StateLayer.Movement);
    }
}