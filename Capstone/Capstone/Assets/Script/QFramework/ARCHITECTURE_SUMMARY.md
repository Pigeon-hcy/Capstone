# QFramework 架构优化总结

## 架构原则

### 1. 职责分离
- **Controller层** - 只负责监听模型变化并直接更新UI显示
- **System层** - 处理所有业务逻辑，直接更新模型
- **State层** - 只负责状态管理和事件发送，不直接调用方法

### 2. 简化的事件驱动
- 业务逻辑通过事件进行通信
- UI更新通过模型绑定自动触发
- 减少不必要的事件转发

## 优化后的架构

### Controller层 (直接更新UI)
```
UIController
├── 监听模型变化 → 直接更新UI组件
├── 不再进行事件转发
└── 职责单一：只负责UI显示
```

### System层 (直接更新模型)
```
TrickSystem
├── 处理技巧执行逻辑
├── 直接更新TrickModel和ScoreModel
└── 不再发送UI相关事件

ScoreSystem
├── 处理分数逻辑
├── 直接更新ScoreModel
└── 不再发送UI相关事件
```

### State层 (只发送事件)
```
AirState
├── 检测落地 → 发送PlayerLandedEvent
├── 检测技巧输入 → 发送TrickInputEvent
└── 不直接调用任何方法

TrickState
├── 执行技巧 → 发送TrickPerformedEvent
├── 检测落地 → 发送PlayerLandedEvent
└── 不直接调用TrickScore方法
```

## 数据流向

### 技巧执行流程 (简化版)
```
1. 玩家按J/K键
2. AirState检测输入 → 发送TrickInputEvent
3. PlayerSystem接收 → 切换到TrickState
4. TrickState执行技巧 → 发送TrickPerformedEvent
5. TrickSystem接收 → 直接更新TrickModel和ScoreModel
6. UIController监听模型变化 → 自动更新UI显示
```

### 玩家落地流程 (简化版)
```
1. 玩家落地
2. AirState/TrickState检测 → 发送PlayerLandedEvent
3. PlayerLandingSystem接收 → 处理落地逻辑
4. PlayerLandingSystem → 调用TrickScore.ResetTrickScore()
5. TrickScore直接更新模型
6. UIController监听模型变化 → 自动更新UI显示
```

## 优势

### 1. 简化架构
- 减少了事件转发层
- 降低了代码复杂度
- 提高了可读性

### 2. 更好的性能
- 减少了事件处理开销
- 直接模型绑定更高效

### 3. 更容易调试
- 数据流向更清晰
- 减少了中间环节

### 4. 更好的维护性
- 职责更明确
- 代码更简洁

## 关键改进

### 1. UIController重构
- 移除了事件转发逻辑
- 直接监听模型变化
- 自动更新UI显示

### 2. System层简化
- 直接更新模型
- 不再发送UI相关事件
- 专注于业务逻辑

### 3. 模型驱动UI
- UI完全由模型状态驱动
- 响应式更新
- 数据一致性更好

## 沟通模式使用

### 1. 模型绑定 (主要使用)
```csharp
// UI更新使用模型绑定
scoreModel.TotalScore.Register(OnScoreChanged);
trickModel.CurrentTricks.Register(OnTrickListChanged);
```

### 2. 事件系统 (状态通知)
```csharp
// 状态变化使用事件系统
this.SendEvent<PlayerLandedEvent>(new PlayerLandedEvent());
this.SendEvent<TrickPerformedEvent>(new TrickPerformedEvent { TrickName = "TrickA" });
```

### 3. 系统调用 (业务逻辑)
```csharp
// 直接业务逻辑使用系统调用
var trickModel = this.GetModel<ITrickModel>();
trickModel.CurrentTricks.Value.Add(newTrick);
```

### 4. 命令模式 (复杂操作)
```csharp
// 复杂操作使用命令模式
this.SendCommand<PerformTrickCommand>(new PerformTrickCommand { TrickName = "TrickA" });
```

详细说明请参考 `COMMUNICATION_PATTERNS.md` 文档。
