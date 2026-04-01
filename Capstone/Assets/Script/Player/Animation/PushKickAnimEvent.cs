using UnityEngine;
using QFramework;
using SkateGame;

// 挂在 Animator 同一 GameObject 上，接收 Push 动画的 Animation Event
public class PushKickAnimEvent : MonoBehaviour
{
    private PlayerController player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
    }


    public void _onPushKick()
    {
        var playerModel = player.GetModel<IPlayerModel>();
        bool pushingRight = playerModel.PushSpeed.Value >= 0f;
        player.SendEvent<PushKickEvent>(new PushKickEvent { IsPushing = true, IsPushingRight = pushingRight });
        AudioManager.Instance.fmodPlayPush();
        if (player.pushEffectPlayer != null)
            player.pushEffectPlayer.PlayFeedbacks();
    }
}
