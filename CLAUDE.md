# UnityDemo_Avatar2 — CLAUDE.md

## 项目概述

Unity 角色换装系统第二版。实现了基于 **SkinnedMeshRenderer** 骨骼绑定的模块化角色换装，支持动态替换网格、材质、动画，以及脸部贴花（Face Decal）功能。

Unity 版本：见 `ProjectSettings/ProjectVersion.txt`

---

## 目录结构

```
Assets/
├── Anims/          # 动画片段（AnimationClip），按角色子目录分类
├── Materials/      # 材质资源，按角色子目录分类
├── Meshs/          # 网格资源（Mesh），按角色子目录分类
├── Resources/      # 运行时动态加载资源（prefab、PartBoneNamesHolder asset）
├── Scenes/         # 场景文件
├── Scripts/        # 运行时 C# 脚本
│   └── Editor/     # 编辑器工具脚本
├── Shader/         # 自定义 Shader（含脸部贴花 Shader）
└── Textures/       # 贴图资源
```

---

## 核心脚本说明

### `Character.cs`

角色系统的核心逻辑，包含两个类：

- **`CharacterPart`**：单个部件（眼睛/脸/发型/裤子/鞋/上衣），持有 `SkinnedMeshRenderer` 引用，负责 `ChangePart()` 时设置 Mesh、材质、Transform、Bounds、骨骼列表和根骨骼。
- **`Character`**：角色整体，管理骨骼 GameObject、`Animation` 组件、`PartBoneNamesHolder` asset，以及所有 `CharacterPart` 的字典。提供 `InitSkeleton`、`InitPartAsset`、`InitPart`、`ChangePart`、`GetBones`、`ChangeAnim` 方法。

部件类型枚举 `CharacterPartType`：`Eyes / Face / Hair / Pants / Shoes / Top`，对应名字数组 `Character.PART_NAMES`。

### `PartBoneNamesHolder.cs`

`ScriptableObject`，存储每个部件的骨骼信息：

- `partName`：部件名（对应 Mesh.name 前缀）
- `rootBoneName`：根骨骼名
- `boneNames[]`：骨骼名列表
- `bounds`：SkinnedMeshRenderer 的 localBounds
- `trans`：部件 Transform（localPos / localRot / localScale）

通过 `GetBoneNames`、`GetBoneRootName`、`GetBounds`、`GetTransInfo` 查询。

### `Main.cs`

MonoBehaviour 入口，负责：

1. `CreateAllAvatarRes()`：扫描 `Assets/Resources/` 下各角色子目录，加载 prefab、PartBoneNamesHolder、Mesh、Material、AnimationClip，构建 `AvatarRes` 列表。
2. `Generate()`：初始化骨骼、部件，调用 `ChangePart`。
3. `OnGUI()`：提供运行时换装交互按钮（切换角色、切换部件 Mesh/材质、切换动画）。

辅助类 `AvatarResPart`：管理单个部件的 Mesh/Material 列表及当前索引。
辅助类 `AvatarRes`：汇总一个角色的全部资源引用。

### `FaceDecal.cs`

静态工具类，实现脸部贴花的 UV 矩阵计算：

- `TransformUV(rotation, translate, scale)`：计算 3×3 UV 变换矩阵（缩放 × 平移 × 旋转 × 锚点平移）。
- `SetMat(material, matrix)`：将矩阵拆成三个 `Vector4`，写入 Shader 属性 `_FaceDecalUVMatrixM0/M1/M2`。

### `FunctionUtil.cs`

静态扩展/工具类：

- `GameObject.Reset(parent)`：重置 Transform，可选设置父节点。
- `CollectAll<T>(path)`：仅编辑器下，从指定路径加载所有指定类型资源（跳过 .meta 文件）。
- `FindChildRecursion(transform, name)`：递归查找子节点。

### `Editor/GenerateResEditor.cs`

编辑器工具，用于生成 `PartBoneNamesHolder` asset（`bonenames.asset`），记录模型 prefab 中各部件的骨骼信息。

---

## 资源规范

- **部件 prefab**：只保留 `Transform` + `SkinnedMeshRenderer`，Mesh / Root Bone / Materials 全部置空，运行时动态填充。
- **骨骼 prefab**（`female_skeleton.prefab`）：只保存骨架层级，不含渲染组件。
- **bonenames.asset**：由编辑器工具生成，存放在 `Resources/<角色名>/` 目录下。
- **Mesh / Material 命名**：部件名需包含对应的 `Character.PART_NAMES` 字符串（如 `eyes`、`top`），供 `AvatarResPart.FillMesh / FillMat` 匹配。
- **材质命名**：需包含对应 Mesh 的名字，供 `FindMatInternalList` 匹配材质与 Mesh 的对应关系。

---

## 开发约定

- 新增部件类型时，同步更新 `CharacterPartType` 枚举、`Character.PART_NAMES` 数组，并重新生成 `bonenames.asset`。
- `FunctionUtil.CollectAll` 仅在 `#if UNITY_EDITOR` 中可用，运行时资源加载需替换为 `Resources.Load` 或 Addressables。
- 脸部贴花 Shader 属性名 (`_FaceDecalUVMatrixM0/M1/M2`) 与 `FaceDecal.cs` 中的 `Shader.PropertyToID` 缓存保持一致。
- 编辑器脚本放在 `Assets/Scripts/Editor/` 目录下，避免打包进运行时。
