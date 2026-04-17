# CameraController 摄像机控制器

## 功能

拖动屏幕水平旋转角色视角，支持 WebGL（鼠标）和移动端（单指触摸）。

## 新增文件

`Assets/Scripts/CameraController.cs`

## Inspector 参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Target | None | 角色骨骼根节点 Transform，运行时自动绑定 |
| Distance | 1.7 | 摄像机与角色的水平距离 |
| Height | 1.08 | 摄像机高度偏移 |
| Look At Ratio | 0.8 | LookAt 目标 Y = height × ratio，控制俯仰角（~7.4°） |
| Rotate Speed | 0.3 | 拖动旋转灵敏度 |
| Drag Threshold | 3 | 触发旋转的最小拖动像素，防止点击 UI 按钮时误旋转 |
| Initial Angle Y | 180 | 初始轨道角（180° = 摄像机在角色正前方） |

## 参数推算依据

参考场景初始摄像机 Transform：
- Position: `(-0.10, 1.076, 1.688)`
- Rotation: `(7.427, 179.45, 0)`

| 参数 | 推算 |
|------|------|
| Distance = 1.7 | `√(1.688² + 0.10²) ≈ 1.69` |
| Height = 1.08 | Position.Y = 1.076 |
| LookAt Ratio = 0.8 | lookAtY = 1.08×0.8 = 0.864，`arctan((1.08-0.864)/1.7) ≈ 7.3°` |
| Initial Angle Y = 180 | Rotation.Y ≈ 179.45° |

## 使用方式

1. 将 `CameraController` 脚本挂载到 **Main Camera**
2. 运行时 `Main.Generate()` 在骨骼创建后自动绑定 `target`，无需手动拖拽
3. `Main.Start()` 通过 `FindObjectOfType<CameraController>()` 自动查找，无需在 Inspector 配置

## 拖动逻辑

- 向右拖动 → 摄像机向左移 → 角色视觉上向右转
- 左侧 UI 面板区域内按下鼠标不触发旋转（`IsPointerInUIPanel` 屏蔽）
- 拖动距离小于 `dragThreshold` 像素不触发旋转，防止点击误操作

## Main.cs 相关修改

```csharp
// Start() 自动查找
if (cameraController == null)
    cameraController = FindObjectOfType<CameraController>();

// Generate() 骨骼加载后立即绑定
if (cameraController != null && mCharacter.skeleton != null)
    cameraController.target = mCharacter.skeleton.transform;

// 切换角色时回正视角
cameraController?.ResetAngle();
```

## WebGL 兼容说明

- 使用 `Input.GetMouseButton` / `Input.GetMouseButtonDown`（WebGL 原生支持）
- 使用 `Input.GetTouch`（移动端浏览器支持）
- 不依赖 `System.IO`、`Thread` 等 WebGL 受限 API
