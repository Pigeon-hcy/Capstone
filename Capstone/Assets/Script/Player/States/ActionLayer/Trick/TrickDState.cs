using UnityEngine;
using System.Collections;
using SkateGame;
using QFramework;

public class TrickDState : TrickState, ICanGetSystem, IBelongToArchitecture
{
    private IEnergySystem energySystem;
    public TrickDState(PlayerController player, Rigidbody2D rb) : base(player, rb)
    {
        isLoop = playerModel.Config.Value.isLoopTrickD;
        stateDuration = playerModel.Config.Value.durationTrickD;
        ignoringMovementLayer = playerModel.Config.Value.ignoringMovementLayerTrickD;
        energySystem = this.GetSystem<IEnergySystem>();
    }

    protected override void EnterTrickState()
    {
        // FOR JERRY'S AUDIO - HOOK USE
        /// TODO：自瞄敌人 
        float direction = inputModel.Move.Value.x != 0f ? (inputModel.Move.Value.x < 0f ? -1f : 1f) : playerModel.IsFacingRight.Value ? 1f : -1f;
        player.vineGun.FireGrabbingHook(90f - 45f * direction);
        //playGrappleShoot();
    }

    public void playGrappleShoot()
    {
        AudioManager.Instance.fmodPlayGrappleShoot();
    }

    protected override void UpdateActionState()
    {
    }

    protected override void ExitActionState()
    {
    } 
}