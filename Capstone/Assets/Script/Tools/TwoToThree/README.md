# Tilemap → 3D Mesh Extruder

Unity Editor 工具：把 2D Tilemap 沿 Z 轴挤出，生成带轮廓的 3D Mesh Prefab。

## 文件结构

```
TilemapTo3DMesh/
└── Editor/
    ├── TilemapMeshBuilder.cs     ← 核心算法（轮廓提取 + Mesh 生成）
    └── TilemapTo3DMeshEditor.cs  ← Editor 窗口 UI
```

## 安装

把 `TilemapTo3DMesh/` 文件夹整体拖进你的 Unity 项目的 `Assets/` 目录下即可。  
（必须放在 `Editor/` 子文件夹内，或保持现有结构不变）

## 使用

1. 菜单栏 → **Tools → Tilemap To 3D Mesh** 打开工具窗口。
2. 把场景中的 **Tilemap** 组件拖入 "Source Tilemap" 槽。
3. 设置 **Extrusion Depth**（Z 轴厚度，默认 1）。
4. 设置 **Asset Name**（生成的 Mesh 和 Prefab 的名字）。
5. 设置 **Save Directory**（保存路径，默认 `Assets/GeneratedMeshes`）。
6. 勾选 **Preview in Scene** 可以在 Scene 视图中看蓝色线框预览。
7. 点击 **Generate & Save Prefab** → 自动生成 `.mesh` + `.prefab`，并在 Project 窗口高亮显示。

## 算法说明

| 步骤 | 内容 |
|------|------|
| 1. 收集填充格子 | 遍历 `Tilemap.cellBounds`，记录有 Tile 的格子坐标 |
| 2. 提取轮廓边 | 对每个格子检查 4 个邻居，空邻居 → 产生一条有向边界边 |
| 3. 走轮廓环 | 用半边图结构把边界边串成闭合轮廓 Loop |
| 4. 前/后盖 | 对每个 Loop 做 **Ear-Clipping 三角剖分**，生成前面和后面 |
| 5. 侧面 | 每段轮廓边生成一个 Quad（2 个三角形），法线朝外 |
| 6. 组装 | 合并所有顶点/UV/法线/三角形，输出最终 Mesh |

## 注意

- Tilemap 需要先 **CompressBounds**（工具会自动调用）。
- 不支持带洞的 Tilemap（内部空格子会被当作独立轮廓处理，效果取决于形状）。
- 生成的 Material 使用 Unity 内置 Default-Diffuse，替换成你项目的材质即可。
- 顶点数超过 65535 时自动切换到 UInt32 索引格式。
