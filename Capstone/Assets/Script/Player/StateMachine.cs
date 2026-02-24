using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
// 状态机类
public class E
{
    // 当前状态
    private StateBase currentState;
    
    // 状态字典，用于存储所有状态
    private Dictionary<Type, StateBase> states = new Dictionary<Type, StateBase>();
    
    
    // 构造函数
    public E()
    {
        currentState = null;
    }
    
    // 添加状态
    public void AddState( StateBase state)
    {
        var key = state.GetType();
        if (!states.ContainsKey(key))
        {
            states.Add(key, state);
        }
    }
    
    public void SwitchState<T>() where T : StateBase
    {   
        var targetType = typeof(T);
        if (currentState == null)
        {
            EnterState(targetType);
        }
        else if (currentState.GetType() != targetType)
        {
            currentState.Exit();
            EnterState(targetType);
        }
    }
    
    public void ReenterCurrentState()
    {
        if (currentState == null)
        {
            return;
        }

        var targetState = currentState.GetType();
        currentState.Exit();
        EnterState(targetState);
    }
    
    // 获取当前状态
    public StateBase GetCurrentState()
    {
        return currentState;
    }
    
    // 获取状态名称
    public string GetCurrentStateName()
    {
        if (currentState != null)
        {
            return currentState.GetType().Name;
        }
        return "Unknown";
    }
    
    // 检查当前状态是否为指定状态
    public bool IsCurrentState<T>() where T : StateBase
    {
        var t = typeof(T);
        return states.ContainsKey(t) && states[t] == currentState;
    }
    
    // 获取所有状态名称
    public string[] GetAllStateNames()
    {
        return states.Keys.Select(k => k.Name).ToArray();
    }
    
    // 更新当前状态
    public void UpdateCurrentState()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }
    
    // 清除所有状态
    public void ClearStates()
    {
        states.Clear();
        currentState = null;
    }


    // 进入新状态
    public void EnterState(Type stateType)
    {
        currentState = states[stateType];
        currentState.Enter();
        Debug.Log("EnterState: " + stateType.Name);
    }
    public StateBase TryGetState(Type stateType)
    {
        if (states.ContainsKey(stateType))
        {
            return states[stateType];
        }
        return null;
    }
} 