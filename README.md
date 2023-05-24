
# Unity的Avatar换装第二版

## Demo示例

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/1.gif?raw=true)

## 第一版换装链接

[UnityDemo_Avatar](https://github.com/xieliujian/UnityDemo_Avatar)

## 优化内容，使在实际项目中使用

### 资源准备

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/1.png?raw=true)

#### 模型prefab拆分成基础的的prefab组件

- `eyes.prefab`
- `face.prefab`
- `hair.prefab`
- `pants.prefab`
- `shoes.prefab` 
- `top.prefab`

以一个prefab为例

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/2.png?raw=true)

prefab只有 `Transform` 和 `SkinnedMeshRenderer` 组件，对于`SkinnedMeshRenderer`组件，内部`Mesh`,`Root Bone`, `Materials`全部置空，资源通过动态加载

#### 骨骼prefab

`female_skeleton.prefab`

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/3.png?raw=true)

骨骼的prefab只保存骨架信息

#### 骨骼资源asset

`bonenames.asset`

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/4.png?raw=true)

```cs

[Serializable]
public class TransInfo
{
    public Vector3 localPos;
    public Quaternion localRot;
    public Vector3 localScale;
}

[Serializable]
public struct Info
{
    public string partName;
    public string rootBoneName;
    public string[] boneNames;

    public Bounds bounds;
    public TransInfo trans;
}

```

记录 `partName`, `rootBoneName`, `boneNames`, `bounds`, `trans`信息, 然后通过查询的方式，填充进入相应的prefab中

### 重点代码

```cs

public void ChangePart(Mesh mesh, List<Material> matList)
{
    if (m_Character == null || m_SkinMesh == null)
        return;

    var partAsset = m_Character.partAsset;
    if (partAsset == null)
        return;

    var partName = mesh.name;

    // 设置Mesh
    m_SkinMesh.sharedMesh = mesh;

    // 设置材质 (真实项目里面，这里动态加载材质路径列表，然后填充进入列表)
    // 例如 mat1path|mat2path|mat3path, 加载这些资源，然后加入列表里面
    m_MatList.Clear();
    m_MatList.AddRange(matList);
    m_SkinMesh.sharedMaterials = m_MatList.ToArray();

    // 设置transform
    var trans = partAsset.GetTransInfo(partName);
    if (trans != null)
    {
        m_SkinMesh.transform.localPosition = trans.localPos;
        m_SkinMesh.transform.localRotation = trans.localRot;
        m_SkinMesh.transform.localScale = trans.localScale;
    }

    // 设置bounds
    m_SkinMesh.localBounds = partAsset.GetBounds(partName);

    // 设置骨骼列表和根骨骼
    m_SkinMesh.bones = m_Character.GetBones(partName, out Transform rootBone);
    m_SkinMesh.rootBone = rootBone;
}

```


