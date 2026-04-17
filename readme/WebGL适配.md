# WebGL 打包适配

## 问题

原始 `CreateAllAvatarRes` 在 WebGL 打包后资源全部加载失败。

## 原因

| 问题点 | 说明 |
|--------|------|
| `DirectoryInfo` / `System.IO` | WebGL 运行在浏览器沙箱，无本地文件系统 |
| `FunctionUtil.CollectAll<T>` | 被 `#if UNITY_EDITOR` 包裹，打包后返回空列表 |
| Mesh / Material / Anim 不在 Resources 下 | 原路径 `Assets/Meshs/`、`Assets/Materials/`、`Assets/Anims/` 打包时不被包含 |

## 修改文件

### `Assets/Scripts/Main.cs`

- 删除 `using System.IO` / `using System`
- `AvatarRes` 类改动：
  - 移除 `MESH_PATH`、`ANIM_PATH`、`MAT_PATH`
  - 新增 `Prefab_PATH = "Assets/Resources/"`
  - 新增 `MESH_SUBPATH = "Meshs"`、`ANIM_SUBPATH = "Anims"`、`MAT_SUBPATH = "Materials"`
  - 新增 `CHAR_NAMES = { "Female", "Male" }`（替代运行时扫描目录）
- `CreateAllAvatarRes` 改用 `Resources.LoadAll<T>` 替代 `FunctionUtil.CollectAll`

### `Assets/Scripts/Editor/GenerateResEditor.cs`

- 删除对 `ANIM_PATH`、`MESH_PATH` 的引用
- 所有生成路径改为 `Prefab_PATH + middir + "/" + MESH_SUBPATH/ANIM_SUBPATH + "/"`
- 生成工具现在直接输出到 `Assets/Resources/<角色名>/Meshs|Anims/`，无需手动迁移

## 资源目录结构（迁移后）

```
Assets/Resources/
├── Female/
│   ├── bonenames.asset
│   ├── female_skeleton.prefab
│   ├── eyes / face / hair / pants / shoes / top .prefab
│   ├── Meshs/       ← .mesh 文件
│   ├── Materials/   ← .mat 文件
│   └── Anims/       ← .anim 文件
└── Male/
    └── ...（同上）
```

## 资源迁移方式

**方式一（推荐）**：在 Project 窗口选中 `Assets/UnuseRes/`，执行菜单 **Avatar2 → Generate**，工具直接生成到正确路径。

**方式二**：在 Unity Editor Project 窗口手动拖拽（不能用 Explorer，否则 .meta 丢失）：

```
Assets/Meshs/Female/      →  Assets/Resources/Female/Meshs/
Assets/Materials/Female/  →  Assets/Resources/Female/Materials/
Assets/Anims/Female/      →  Assets/Resources/Female/Anims/
```

## 新增角色

在 `AvatarRes.CHAR_NAMES` 数组追加角色目录名，按上述结构放置资源即可。
