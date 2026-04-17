# OnGUI 屏幕自适应

## 问题

原始 `Main.OnGUI` 使用固定像素尺寸（`typeWidth=240`、`typeheight=100`、`buttonWidth=60`、`fontSize=50`），在不同分辨率/WebGL 页面大小下 UI 显示异常。

## 方案

使用 `GUI.matrix` 对整个 IMGUI 做等比缩放，以参考分辨率 **1920×1080** 为基准，自动适配任何屏幕尺寸。

## 修改文件

### `Assets/Scripts/Main.cs`

新增参考分辨率常量：

```csharp
const float REF_WIDTH  = 1920f;
const float REF_HEIGHT = 1080f;
```

`OnGUI` 首行添加缩放矩阵：

```csharp
float scale = Mathf.Min(Screen.width / REF_WIDTH, Screen.height / REF_HEIGHT);
GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
```

- `Mathf.Min` 取宽高缩放比的较小值，等比缩放不变形
- GUI 元素尺寸值保持不变，由矩阵统一缩放

## 各分辨率缩放效果

| 实际分辨率 | scale | 按钮高度实际像素 |
|-----------|-------|----------------|
| 1920×1080 | 1.0   | 100px          |
| 1280×720  | 0.667 | ~67px          |
| 960×540   | 0.5   | 50px           |
