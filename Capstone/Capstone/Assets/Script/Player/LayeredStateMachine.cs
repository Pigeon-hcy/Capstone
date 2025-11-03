using System.Collections.Generic;
using QFramework;
using SkateGame;

public enum StateLayer
{
    Movement,
    Action
}
public class LayeredStateMachine : ICanGetModel, ICanSendEvent, IBelongToArchitecture
{
    private readonly IPlayerModel playerModel;
    private readonly E mMovement = new E();
    private readonly E mAction = new E();
    
    /// <summary>
    /// 判断状态属于哪一层
    /// </summary>
    private readonly Dictionary<string, StateLayer> mStateToLayer = new Dictionary<string, StateLayer>();
    public IArchitecture GetArchitecture() => SkateGame.GameApp.Interface;
    public LayeredStateMachine()
    {
        playerModel = this.GetModel<IPlayerModel>();
    }
    public void AddState(string stateName, StateBase state, StateLayer layer)
    {
        if (!mStateToLayer.ContainsKey(stateName))
        {
            mStateToLayer.Add(stateName, layer);
        }
        else
        {
            mStateToLayer[stateName] = layer;
        }

        if (layer == StateLayer.Movement)
        {
            mMovement.AddState(stateName, state);
        }
        else
        {
            mAction.AddState(stateName, state);
        }
    }

    public void SwitchState(StateLayer layer, string stateName)
    {
        if (layer == StateLayer.Movement)
        {
            mMovement.SwitchState(stateName);
            this.SendEvent<StateChangedEvent>(new StateChangedEvent { Layer = StateLayer.Movement, 
                FromState = mMovement.GetCurrentStateName(), ToState = stateName });
        }
        else
        {
            mAction.SwitchState(stateName);
            this.SendEvent<StateChangedEvent>(new StateChangedEvent { Layer = StateLayer.Action, 
                FromState = mAction.GetCurrentStateName(), ToState = stateName });
        }
    }

    public void UpdateCurrentState()
    {
        // Update action first
        mAction.UpdateCurrentState();
        mMovement.UpdateCurrentState();
    }

    public string GetMovementStateName()
    {
        return mMovement.GetCurrentStateName();
    }

    public string GetActionStateName()
    {
        return mAction.GetCurrentStateName();
    }

    public StateBase TryGetState(string stateName, StateLayer layer )
    {
        if (layer == StateLayer.Movement)
        {
            return mMovement.TryGetState(stateName);
        }
        else
        {
            return mAction.TryGetState(stateName);
        }
    }
}


