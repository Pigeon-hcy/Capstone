# QFramework 沟通模式完整指南

## 📋 概述

本文档详细记录了QFramework中所有的信息传递模式，以及当前项目中的具体使用情况。

## 🎯 沟通模式分类

### 1. 事件系统 (Event System)
**用途：** 一次性通知，适合状态变化和动作通知

#### 基本语法
```csharp
// 发送事件
this.SendEvent<ScoreUpdatedEvent>(new ScoreUpdatedEvent { NewScore = 100 });

// 监听事件
this.RegisterEvent<ScoreUpdatedEvent>(OnScoreUpdated);

private void OnScoreUpdated(ScoreUpdatedEvent evt)
{
    Debug.Log($"收到事件: {evt.NewScore}");
}
```

#### 特点
- ✅ 一次性通知
- ✅ 发送后立即触发
- ✅ 适合"动作"类型的通知
- ✅ 需要手动发送和监听
- ❌ 可能遗漏发送
- ❌ 需要手动管理生命周期

#### 当前项目使用情况
```csharp
// 在状态机中使用
this.SendEvent<PlayerLandedEvent>(new PlayerLandedEvent());
this.SendEvent<TrickPerformedEvent>(new TrickPerformedEvent { TrickName = "TrickA" });

// 在系统中监听
this.RegisterEvent<PlayerLandedEvent>(OnPlayerLanded);
this.RegisterEvent<TrickPerformedEvent>(OnTrickPerformed);
```

---

### 2. 模型绑定 (Model Binding) - 推荐使用
**用途：** 持续监听数据变化，适合UI更新和数据同步

#### 基本语法
```csharp
// 监听模型变化
scoreModel.TotalScore.Register(OnScoreChanged);

// 当模型值变化时自动调用
private void OnScoreChanged(int newScore)
{
    Debug.Log($"分数变化: {newScore}");
}

// 更新模型值
scoreModel.TotalScore.Value = 100;  // 自动触发回调
```

#### 特点
- ✅ 持续监听
- ✅ 值变化时自动触发
- ✅ 适合"数据"类型的监听
- ✅ 响应式编程
- ✅ 不会遗漏更新
- ✅ 自动管理生命周期

#### 当前项目使用情况
```csharp
// UIController 中使用
scoreModel.TotalScore.Register(OnScoreChanged);
trickModel.CurrentTricks.Register(OnTrickListChanged);
trickModel.CurrentTrickName.Register(OnTrickNameChanged);
scoreModel.ComboMultiplier.Register(OnComboChanged);

// 模型更新
trickModel.CurrentTricks.Value.Add(newTrick);
scoreModel.TotalScore.Value = totalScore;
trickModel.CurrentTrickName.Value = trickBase.trickName;
scoreModel.ComboMultiplier.Value = trickModel.CurrentTricks.Value.Count;
```

---

### 3. 命令模式 (Command Pattern)
**用途：** 封装操作逻辑，适合复杂的业务操作

#### 基本语法
```csharp
// 发送命令
this.SendCommand<PerformTrickCommand>(new PerformTrickCommand { TrickName = "TrickA" });

// 命令执行
public class PerformTrickCommand : ICommand
{
    public string TrickName;
    
    public void Execute()
    {
        // 执行技巧逻辑
        Debug.Log($"执行技巧: {TrickName}");
    }
}
```

#### 特点
- ✅ 封装操作逻辑
- ✅ 可以带参数
- ✅ 适合复杂的业务操作
- ✅ 可以撤销/重做
- ✅ 易于测试
- ❌ 增加代码复杂度

#### 当前项目使用情况
```csharp
// 已定义但未大量使用
public class PerformTrickCommand : ICommand, ICanSetArchitecture, IBelongToArchitecture
{
    public string TrickName;
    private IArchitecture mArchitecture;
    
    public void Execute()
    {
        mArchitecture.GetSystem<ITrickSystem>().PerformTrick(TrickName);
    }
}
```

---

### 4. 查询模式 (Query Pattern)
**用途：** 获取数据，不修改状态

#### 基本语法
```csharp
// 发送查询
var result = this.SendQuery<GetScoreQuery>(new GetScoreQuery());

// 查询处理
public class GetScoreQuery : IQuery<int>
{
    public int Do()
    {
        return 100;  // 返回分数
    }
}
```

#### 特点
- ✅ 获取数据
- ✅ 有返回值
- ✅ 适合数据查询
- ✅ 不修改状态
- ✅ 易于测试
- ❌ 增加代码复杂度

#### 当前项目使用情况
```csharp
// 未在当前项目中使用
// 可以用于获取当前分数、技巧列表等
```

---

### 5. 系统调用 (System Call)
**用途：** 直接调用系统方法，适合简单操作

#### 基本语法
```csharp
// 直接调用系统方法
var scoreSystem = this.GetSystem<IScoreSystem>();
scoreSystem.AddScore(100);
```

#### 特点
- ✅ 直接方法调用
- ✅ 同步执行
- ✅ 适合简单操作
- ✅ 代码直观
- ❌ 紧耦合
- ❌ 难以测试

#### 当前项目使用情况
```csharp
// 在TrickScore中使用
var trickModel = this.GetModel<ITrickModel>();
var scoreModel = this.GetModel<IScoreModel>();
trickModel.CurrentTricks.Value.Add(newTrick);
scoreModel.TotalScore.Value = totalScore;
```

---

## 📊 模式对比表

| 模式 | 用途 | 特点 | 适用场景 | 当前使用 |
|------|------|------|----------|----------|
| **事件系统** | 通知 | 一次性、立即触发 | 状态变化、动作通知 | ✅ 大量使用 |
| **模型绑定** | 数据监听 | 持续监听、自动触发 | UI更新、数据同步 | ✅ 主要使用 |
| **命令模式** | 操作封装 | 可撤销、带参数 | 复杂业务操作 | ⚠️ 少量使用 |
| **查询模式** | 数据获取 | 有返回值、不修改状态 | 数据查询 | ❌ 未使用 |
| **系统调用** | 直接调用 | 同步、紧耦合 | 简单操作 | ✅ 部分使用 |

## 🎮 当前项目架构分析

### 重构前 (事件转发模式)
```csharp
// 旧方式：复杂的事件转发链
TrickScore → 发送事件 → UIController → 转发事件 → ScoreDisplaySystem → 更新UI
```

### 重构后 (模型绑定模式) - 推荐
```csharp
// 新方式：直接的模型绑定
TrickScore → 更新模型 → UIController监听模型变化 → 自动更新UI
```

## 🎯 推荐使用原则

### 1. UI更新 → 使用模型绑定
```csharp
// 推荐：UI监听模型变化
scoreModel.TotalScore.Register(OnScoreChanged);
trickModel.CurrentTricks.Register(OnTrickListChanged);
```

### 2. 状态通知 → 使用事件系统
```csharp
// 推荐：状态变化通知
this.SendEvent<PlayerLandedEvent>(new PlayerLandedEvent());
this.SendEvent<TrickPerformedEvent>(new TrickPerformedEvent { TrickName = "TrickA" });
```

### 3. 复杂操作 → 使用命令模式
```csharp
// 推荐：复杂业务逻辑
this.SendCommand<PerformTrickCommand>(new PerformTrickCommand { TrickName = "TrickA" });
```

### 4. 数据查询 → 使用查询模式
```csharp
// 推荐：获取数据
var score = this.SendQuery<GetScoreQuery>(new GetScoreQuery());
```

### 5. 简单调用 → 使用系统调用
```csharp
// 推荐：简单直接的操作
var system = this.GetSystem<IScoreSystem>();
system.AddScore(100);
```

## 🔧 最佳实践

### 1. 优先使用模型绑定
- 对于UI更新，优先使用模型绑定
- 减少事件转发，提高性能
- 确保数据一致性

### 2. 合理使用事件系统
- 用于状态变化通知
- 用于系统间的解耦
- 避免过度使用

### 3. 避免直接系统调用
- 除非是简单的内部调用
- 优先考虑其他模式
- 保持架构的清晰性

### 4. 统一命名规范
```csharp
// 事件命名：XxxEvent
PlayerLandedEvent, TrickPerformedEvent

// 命令命名：XxxCommand  
PerformTrickCommand, ResetScoreCommand

// 查询命名：GetXxxQuery
GetScoreQuery, GetTrickListQuery

// 模型属性命名：Xxx
TotalScore, CurrentTricks, ComboMultiplier
```

## 📝 总结

当前项目主要使用**模型绑定**和**事件系统**两种模式：

- **模型绑定**：用于UI自动更新（主要使用）
- **事件系统**：用于状态变化通知（大量使用）
- **系统调用**：用于直接业务逻辑（部分使用）
- **命令模式**：用于复杂操作（少量使用）
- **查询模式**：未使用，可考虑引入

这种组合使用很好地平衡了代码的解耦性、可维护性和性能。
