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
    private Dictionary<string, StateBase> states = new Dictionary<string, StateBase>();
    
    
    // 构造函数
    public E()
    {
        currentState = null;
    }
    
    // 添加状态
    public void AddState(string stateName, StateBase state)
    {
        if (!states.ContainsKey(stateName))
        {
            states.Add(stateName, state);
        }
    }
    
    public void SwitchState(string stateName)
    {   
        if (currentState == null)
        {
            EnterState(stateName);
        }
        else if (currentState.GetStateName() != stateName)
        {
            StateBase oldState = currentState;
            currentState.Exit();
            EnterState(stateName, oldState);
        }
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
            return currentState.GetStateName();
        }
        return "Unknown";
    }
    
    // 检查当前状态是否为指定状态
    public bool IsCurrentState(string stateName)
    {
        return states.ContainsKey(stateName) && states[stateName] == currentState;
    }
    
    // 获取所有状态名称
    public string[] GetAllStateNames()
    {
        return states.Keys.ToArray();
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
    public void EnterState(string stateName,StateBase oldState = null)
    {
        currentState = states[stateName];
        currentState.Enter();
        Debug.Log("EnterState: " + stateName);
    }
    public StateBase TryGetState(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            return states[stateName];
        }
        return null;
    }
} 