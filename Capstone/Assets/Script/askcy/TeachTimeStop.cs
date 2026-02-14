using UnityEngine;
using MoreMountains.Feedbacks;
using QFramework;
using SkateGame;

public enum TeachTimeStopRequiredInput
{
    Jump,
    Grind,
    Grab,
    Dash,
    Push,
    Brake,
    Shoot,
    SwitchItem
}

public class TeachTimeStop : MonoBehaviour, ICanGetModel, IBelongToArchitecture
{
    public MMF_Player timeStopEffect;
    public TeachTimeStopRequiredInput requireInput;
    private bool isTimeStopped = false;

    public IArchitecture GetArchitecture() => GameApp.Interface;

    private void OnEnable()
    {
        if (timeStopEffect != null && timeStopEffect.Events != null)
            timeStopEffect.Events.OnComplete.AddListener(OnTimeStopEffectStop);
            timeStopEffect.Events.OnStop.AddListener(OnTimeStopEffectStop);
    }

    private void OnDisable()
    {
        if (timeStopEffect != null && timeStopEffect.Events != null)
            timeStopEffect.Events.OnComplete.RemoveListener(OnTimeStopEffectStop);
            timeStopEffect.Events.OnStop.RemoveListener(OnTimeStopEffectStop);
    }

    private void OnTimeStopEffectStop()
    {
        isTimeStopped = false;
        ExitTutorial();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag != "Player") return;
        if (GameStateController.Instance == null || GameStateController.Instance.Current != GameState.InGame) return;

        isTimeStopped = true;
        GameStateController.Instance.EnterTutorial();
        timeStopEffect.PlayFeedbacks();
    }

    private void Update()
    {
        if (!isTimeStopped) return;

        var inputModel = this.GetModel<IInputModel>();
        if (GetRequiredInputPressed(inputModel))
        {
            isTimeStopped = false;
            timeStopEffect.StopFeedbacks();
        }
    }

    private void ExitTutorial()
    {
        if (GameStateController.Instance != null && GameStateController.Instance.Current == GameState.Tutorial)
            GameStateController.Instance.EnterInGame();
    }

    private bool GetRequiredInputPressed(IInputModel inputModel)
    {
        if (inputModel == null) return false;

        switch (requireInput)
        {
            case TeachTimeStopRequiredInput.Jump: return inputModel.JumpStart.Value;
            case TeachTimeStopRequiredInput.Grind: return inputModel.GrindStart.Value;
            case TeachTimeStopRequiredInput.Grab: return inputModel.GrabStart.Value;
            case TeachTimeStopRequiredInput.Dash: return inputModel.DashStart.Value;
            case TeachTimeStopRequiredInput.Push: return inputModel.PushStart.Value;
            case TeachTimeStopRequiredInput.Brake: return inputModel.BrakeStart.Value;
            case TeachTimeStopRequiredInput.Shoot: return inputModel.ShootStart.Value;
            case TeachTimeStopRequiredInput.SwitchItem: return inputModel.SwitchItem.Value;
            default: return false;
        }
    }
}
