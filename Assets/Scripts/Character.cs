using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterPartType
{
    Eyes,
    Face,
    Hair,
    Pants,
    Shoes,
    Top,
    TotalNum,
}

public class CharacterPart
{
    /// <summary>
    /// 角色
    /// </summary>
    Character m_Character;

    /// <summary>
    /// 节点
    /// </summary>
    GameObject m_GO;

    /// <summary>
    /// .
    /// </summary>
    SkinnedMeshRenderer m_SkinMesh;

    /// <summary>
    /// 部件类型
    /// </summary>
    public int partType;

    public void Init(Character cha, GameObject partPrefab)
    {
        m_Character = cha;

        if (m_GO == null)
        {
            m_GO = GameObject.Instantiate(partPrefab);
            m_GO.Reset(m_Character.skeleton);
            m_GO.name = partPrefab.name;

            m_SkinMesh = m_GO.GetComponent<SkinnedMeshRenderer>();
        }
    }

    public void ChangePart(Mesh mesh, Material mat)
    {
        if (m_Character == null || m_SkinMesh == null)
            return;

        var partAsset = m_Character.partAsset;
        if (partAsset == null)
            return;

        var partName = mesh.name;
        m_SkinMesh.sharedMesh = mesh;
        m_SkinMesh.sharedMaterial = mat;

        var trans = partAsset.GetTransInfo(partName);
        if (trans != null)
        {
            m_SkinMesh.transform.localPosition = trans.localPos;
            m_SkinMesh.transform.localRotation = trans.localRot;
            m_SkinMesh.transform.localScale = trans.localScale;
        }

        m_SkinMesh.localBounds = partAsset.GetBounds(partName);

        m_SkinMesh.bones = m_Character.GetBones(partName, out Transform rootBone);
        m_SkinMesh.rootBone = rootBone;

        //m_SkinMesh.rootBone = partAsset.GetBoneRootName(partName);
    }

    //public void ChangeEquip(AvatarRes avatarres)
    //{

    //}

    //void ShareSkeleton()
    //{
    //    if (m_Character == null || m_SkinMesh == null)
    //        return;

    //    var partAsset = m_Character.partAsset;
    //    if (partAsset == null)
    //        return;


    //}

    //void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    //{
    //    Transform[] newBones = new Transform[selfSkin.bones.Length];
    //    for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
    //    {
    //        GameObject bone = selfSkin.bones[i].gameObject;
    //        if (bone == null)
    //            continue;

    //        // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
    //        newBones[i] = FunctionUtil.FindChildRecursion(target.transform, bone.name);
    //    }

    //    selfSkin.bones = newBones;
    //}
}

public class Character
{
    /// <summary>
    /// 部件名定义
    /// </summary>
    public static string[] PART_NAMES =
    {
        "eyes",
        "face",
        "hair",
        "pants",
        "shoes",
        "top",
    };

    /// <summary>
    /// asset名字
    /// </summary>
    public const string BONE_ASSET_NAME = "bonenames";

    /// <summary>
    /// 骨骼名字
    /// </summary>
    public const string SKELETON_NAME = "_skeleton";

    /// <summary>
    /// 骨骼
    /// </summary>
    GameObject m_Skeleton;

    /// <summary>
    /// 动画
    /// </summary>
    Animation m_Anim;

    /// <summary>
    /// 资源
    /// </summary>
    PartBoneNamesHolder m_PartAsset;

    /// <summary>
    /// 部件列表
    /// </summary>
    Dictionary<int, CharacterPart> m_PartDict = new Dictionary<int, CharacterPart>();

    /// <summary>
    /// .
    /// </summary>
    static List<Transform> s_TempTransList = new List<Transform>();

    /// <summary>
    /// .
    /// </summary>
    Dictionary<string, Transform> m_BoneTransMap = new Dictionary<string, Transform>();

    /// <summary>
    /// 名字
    /// </summary>
    public string charName;

    /// <summary>
    /// .
    /// </summary>
    public GameObject skeleton
    {
        get { return m_Skeleton; }
    }

    /// <summary>
    /// .
    /// </summary>
    public PartBoneNamesHolder partAsset
    {
        get { return m_PartAsset; }
    }

    /// <summary>
    /// 初始化骨骼
    /// </summary>
    /// <param name="skeleton"></param>
    public void InitSkeleton(GameObject skeleton)
    {
        Clear();

        m_Skeleton = GameObject.Instantiate(skeleton);
        m_Skeleton.Reset(null);
        m_Skeleton.name = charName;

        m_Anim = m_Skeleton.GetComponent<Animation>();

        InitBoneMap();
    }

    /// <summary>
    /// 初始化Asset资源
    /// </summary>
    /// <param name="boneAsset"></param>
    public void InitPartAsset(PartBoneNamesHolder boneAsset)
    {
        m_PartAsset = boneAsset;
    }

    /// <summary>
    /// 初始化部件
    /// </summary>
    /// <param name="partType"></param>
    /// <param name="partGo"></param>
    public void InitPart(int partType, GameObject partGo)
    {
        CharacterPart part = null;
        m_PartDict.TryGetValue(partType, out part);

        if (part == null)
        {
            part = new CharacterPart();
            m_PartDict.Add(partType, part);

            part.partType = partType;
            part.Init(this, partGo);
        }
    }

    public void ChangePart(int partType, Mesh mesh, Material mat)
    {
        CharacterPart part = null;
        m_PartDict.TryGetValue(partType, out part);

        if (part == null)
            return;

        part.ChangePart(mesh, mat);
    }

    /// <summary>
    /// 获取骨骼
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="rootBone"></param>
    /// <returns></returns>
    public Transform[] GetBones(string partName,  out Transform rootBone)
    {
        rootBone = null;
        if (string.IsNullOrEmpty(partName) || m_PartAsset == null || s_TempTransList == null)
        {
            return null;
        }

        s_TempTransList.Clear();
        string[] boneNames = m_PartAsset.GetBoneNames(partName);
        for (int i = 0; i < boneNames.Length; ++i)
        {
            if (m_BoneTransMap.TryGetValue(boneNames[i], out var boneTrans))
            {
                s_TempTransList.Add(boneTrans);
            }
        }

        string rootBoneName = m_PartAsset.GetBoneRootName(partName);
        if (!string.IsNullOrEmpty(rootBoneName))
        {
            m_BoneTransMap.TryGetValue(rootBoneName, out rootBone);
        }

        return s_TempTransList.ToArray();
    }

    /// <summary>
    /// .
    /// </summary>
    /// <param name="clip"></param>
    public void ChangeAnim(AnimationClip clip)
    {
        if (m_Anim == null)
            return;

        m_Anim.wrapMode = WrapMode.Loop;
        m_Anim.Play(clip.name);
    }

    /// <summary>
    /// .
    /// </summary>
    void Clear()
    {
        m_BoneTransMap.Clear();

        if (m_Skeleton != null)
        {
            GameObject.Destroy(m_Skeleton);
        }

        m_PartDict.Clear();
    }

    /// <summary>
    /// .
    /// </summary>
    void InitBoneMap()
    {
        if (m_BoneTransMap.Count <= 0)
        {
            m_BoneTransMap.Clear();
            s_TempTransList.Clear();
            m_Skeleton.GetComponentsInChildren<Transform>(true, s_TempTransList);
            for (int i = 0; i < s_TempTransList.Count; ++i)
            {
                m_BoneTransMap[s_TempTransList[i].name] = s_TempTransList[i];
            }
        }
    }

    //public void Generate(AvatarRes avatarres)
    //{
    //    //DestroyAll();

    //    //m_Skeleton = GameObject.Instantiate(avatarres.mSkeleton);
    //    //m_Skeleton.Reset(null);
    //    //m_Skeleton.name = avatarres.mSkeleton.name;

    //    //m_Anim = m_Skeleton.GetComponent<Animation>();

    //    //m_PartAsset = avatarres.mBoneHolder;

    //    //for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
    //    //{
    //    //    ChangeEquip(i, avatarres);
    //    //}

    //    //ChangeAnim(avatarres);
    //}

    //public void ChangeAnim(AvatarRes avatarres)
    //{
    //    if (m_Anim == null)
    //        return;

    //    //AnimationClip animclip = avatarres.mAnimList[avatarres.mAnimIdx];
    //    //m_Anim.wrapMode = WrapMode.Loop;
    //    //m_Anim.Play(animclip.name);
    //}

    //void DestroyAll()
    //{
    //    if (m_Skeleton != null)
    //    {
    //        GameObject.DestroyImmediate(m_Skeleton);
    //    }

    //    m_PartDict.Clear();
    //}

    //void ChangeEquip(ref GameObject go, GameObject resgo)
    //{
    //    if (go != null)
    //    {
    //        GameObject.DestroyImmediate(go);
    //    }

    //    go = GameObject.Instantiate(resgo);
    //    go.Reset(m_Skeleton);
    //    go.name = resgo.name;

    //    SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
    //    ShareSkeletonInstanceWith(render, m_Skeleton);
    //}

    ///// <summary>
    ///// 共享骨骼
    ///// </summary>
    ///// <param name="selfSkin"></param>
    ///// <param name="target"></param>
    //void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    //{
    //    Transform[] newBones = new Transform[selfSkin.bones.Length];
    //    for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
    //    {
    //        GameObject bone = selfSkin.bones[i].gameObject;
    //        if (bone == null)
    //            continue;

    //        // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
    //        newBones[i] = FunctionUtil.FindChildRecursion(target.transform, bone.name);
    //    }

    //    selfSkin.bones = newBones;
    //}
}
