using UnityEngine;
using QFramework;
using SkateGame;

// 状态基类
public abstract class StateBase : ICanGetModel, IBelongToArchitecture
{
    protected PlayerController player;
    protected Rigidbody2D rb;
    protected IPlayerModel playerModel;
    protected IInputModel inputModel;

    protected StateBase()
    {
        playerModel = this.GetModel<IPlayerModel>();
        inputModel = this.GetModel<IInputModel>();
    }
    // 进入状态时调用
    public virtual void Enter() { }
    
    // 状态更新时调用
    public virtual void Update() { }
    
    // 退出状态时调用
    public virtual void Exit() { }
    
    // 获取状态名称
    public abstract string GetStateName();

    public IArchitecture GetArchitecture() => SkateGame.GameApp.Interface;
    
} 