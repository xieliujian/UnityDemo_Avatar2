
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
    // 例如眼睛等部件，可能会有多个材质，所以这里需要是材质列表
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

## 脸贴花

![github](https://github.com/xieliujian/UnityDemo_Avatar2/blob/main/Video/5.png?raw=true)

贴花矩阵计算代码

```cs

public static Matrix4x4 TransformUV(float rotation, Vector2 translate, float scale)
{
    Matrix4x4 matrix = Matrix4x4.identity;

    // rotate
    float radian = rotation * Mathf.Deg2Rad;
    float cosRadian = Mathf.Cos(radian);
    float sinRadian = Mathf.Sin(radian);
    Matrix4x4 rotateM = new Matrix4x4(new Vector4(cosRadian, sinRadian, 0, 0),
                                        new Vector4(-sinRadian, cosRadian, 0, 0),
                                        new Vector4(0, 0, 1, 0),
                                        new Vector4(0, 0, 0, 1));

    if (scale == 0)
    {
        scale = 1;
    }

    float invScale = 1 / scale;
    float offestX = -translate.x * invScale - 0.5f * invScale;
    float offsetY = -translate.y * invScale - 0.5f * invScale;

    // anchor translate
    Matrix4x4 preTranslateM = new Matrix4x4(new Vector4(1, 0, 0.5f, 0),
                                            new Vector4(0, 1, 0.5f, 0),
                                            new Vector4(0, 0, 1, 0),
                                            new Vector4(0, 0, 0, 1));

    // translate
    Matrix4x4 translateM = new Matrix4x4(new Vector4(1, 0, offestX, 0),
                                        new Vector4(0, 1, offsetY, 0),
                                        new Vector4(0, 0, 1, 0),
                                        new Vector4(0, 0, 0, 1));

    // scale
    Matrix4x4 scaleM = new Matrix4x4(new Vector4(invScale, 0, 0, 0),
                                    new Vector4(0, invScale, 0, 0),
                                    new Vector4(0, 0, 1, 0),
                                    new Vector4(0, 0, 0, 1));

    matrix = scaleM * translateM * rotateM * preTranslateM;

    return matrix;
}

```

贴花shader计算

```cs

half4 CalcDecal(float2 uv)
{
    half3x3 calMatrix = half3x3(_FaceDecalUVMatrixM0.xyz,
        _FaceDecalUVMatrixM1.xyz,
        _FaceDecalUVMatrixM2.xyz);

    uv = saturate(mul(calMatrix, half3(uv, 1)).xy);
    half4 decalColor = tex2D(_FaceDecalTex, uv) * _FaceDecalColor;

    return decalColor;
}

```