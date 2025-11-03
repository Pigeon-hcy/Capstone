using UnityEngine;
using SkateGame;
using QFramework;

public class GrindState : ActionStateBase
{    private float speed;
    private Vector2 direction;
    private float normalG;

    private TrackDirComputeTool trackRef;

    private int leftToRight = 1;

    public GrindState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        speed = playerModel.Speed.Value;
        direction = playerModel.GrindDirection.Value;
        normalG = playerModel.NormalG.Value;
        isLoop = playerModel.Config.Value.isLoopGrind;
        ignoreMovementLayerDuration = playerModel.Config.Value.ignoreMovementLayerDurationGrind;
        trackRef = null;
    }

    public override string GetStateName() => "Grind";

    protected override void EnterActionState()
    {
        playerModel.CanDoubleJump.Value = true;
        // 检查currentTrack是否为null
        if (playerModel.CurrentTrack.Value == null)
        {
            Debug.LogError("GrindState.Enter: currentTrack为null，无法进入滑轨状态");
            player.stateMachine.SwitchState(StateLayer.Action, "Jump");
            return;
        }
        
        Vector2 velocity = rb.linearVelocity;
        speed = velocity.magnitude;
        normalG = rb.gravityScale;
        if (speed < 0.1f)
        {
            Vector2 trackDir = playerModel.CurrentTrack.Value.GetTrackDirection();
            direction = new Vector2(trackDir.x, 0).normalized;
            leftToRight = velocity.x > 0 ? 1 : -1;
            direction *= leftToRight;
            speed = playerModel.Config.Value.maxMoveSpeed;
            
        }
        else
        {
            direction = new Vector2(velocity.x, 0).normalized;
            leftToRight = velocity.x > 0 ? 1 : -1;
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction.x * speed, 0);
        trackRef = playerModel.CurrentTrack.Value.GetDirTool();
        SnapPlayerToTrack();
        
        if (player.GrindEffect != null)
        {
            player.GrindEffect.PlayFeedbacks();
        }
    }

    protected override void UpdateActionState()
    {
        // 首先检查E键是否按住，如果没有按住立即退出
        if (!inputModel.Grind.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
            player.stateMachine.SwitchState(StateLayer.Movement, "Jump");
            return;
        }
        
        // 如果按下跳跃键，直接跳跃
        if (inputModel.JumpStart.Value)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
            player.stateMachine.SwitchState(StateLayer.Movement, "Jump");
            return;
        }
        // 然后检查currentTrack是否为null
        if (playerModel.CurrentTrack.Value == null)
        {
            player.stateMachine.SwitchState(StateLayer.Action, "None");
            player.stateMachine.SwitchState(StateLayer.Movement, "Jump");
            return;
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
            player.transform.position = player.transform.position + new Vector3(movement.x,movement.y, 0);
            pos = trackRef.GetNearestPointAndTangent(player.transform.position, leftToRight, recorcPos, hintTangent: recordDir).nearest;


            //pos.x += moveDelta.x;
            //pos.y = playerModel.CurrentTrack.Value.GetTrackPosition().y+0.2f;
            player.transform.position = pos;

            rb.linearVelocity = direction*playerModel.Config.Value.maxMoveSpeed;
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
        rb.gravityScale = normalG;

        if (player.GrindEffect != null)
        {
            player.GrindEffect.StopFeedbacks();

        }
    }
}