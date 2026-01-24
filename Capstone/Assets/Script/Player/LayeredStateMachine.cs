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
    private readonly Dictionary<System.Type, StateLayer> mStateToLayer = new Dictionary<System.Type, StateLayer>();
    public IArchitecture GetArchitecture() => SkateGame.GameApp.Interface;
    public LayeredStateMachine()
    {
        playerModel = this.GetModel<IPlayerModel>();
    }
    public void AddState(StateBase state, StateLayer layer)
    {
        var key = state.GetType();
        if (!mStateToLayer.ContainsKey(key))
        {
            mStateToLayer.Add(key, layer);
        }
        else
        {
            mStateToLayer[key] = layer;
        }

        if (layer == StateLayer.Movement)
        {
            mMovement.AddState(state);
        }
        else
        {
            mAction.AddState(state);
        }
    }

    public void SwitchState<T>(StateLayer layer) where T : StateBase
    {
        if (layer == StateLayer.Movement)
        {
            mMovement.SwitchState<T>();
            this.SendEvent<StateChangedEvent>(new StateChangedEvent { Layer = StateLayer.Movement, 
                FromState = mMovement.GetCurrentStateName(), ToState = typeof(T).Name });
        }
        else
        {
            var from = mAction.GetCurrentStateName();
            playerModel.LastActionStateName.Value = from;
            mAction.SwitchState<T>();
            this.SendEvent<StateChangedEvent>(new StateChangedEvent { Layer = StateLayer.Action, 
                FromState = from, ToState = typeof(T).Name });
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

    public StateBase TryGetState<T>(StateLayer layer ) where T : StateBase
    {
        if (layer == StateLayer.Movement)
        {
            return mMovement.TryGetState(typeof(T));
        }
        else
        {
            return mAction.TryGetState(typeof(T));
        }
    }
}


