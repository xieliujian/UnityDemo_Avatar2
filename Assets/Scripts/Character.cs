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

    public void ChangeEquip(AvatarRes avatarres)
    {

    }

    void ShareSkeleton()
    {
        if (m_Character == null || m_SkinMesh == null)
            return;

        var partAsset = m_Character.partAsset;
        if (partAsset == null)
            return;


    }

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

    public GameObject skeleton
    {
        get { return m_Skeleton; }
    }

    public PartBoneNamesHolder partAsset
    {
        get { return m_PartAsset; }
    }

    public void Generate(AvatarRes avatarres)
    {
        DestroyAll();

        m_Skeleton = GameObject.Instantiate(avatarres.mSkeleton);
        m_Skeleton.Reset(null);
        m_Skeleton.name = avatarres.mSkeleton.name;

        m_Anim = m_Skeleton.GetComponent<Animation>();

        m_PartAsset = avatarres.mBoneHolder;

        for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
        {
            ChangeEquip(i, avatarres);
        }

        ChangeAnim(avatarres);
    }

    public void ChangeEquip(int partType, AvatarRes avatarres)
    {
        if (partType < 0 || partType >= avatarres.partList.Count)
            return;

        CharacterPart part = null;
        m_PartDict.TryGetValue(partType, out part);

        if (part != null)
        {
            part.ChangeEquip(avatarres);
        }
        else
        {
            part = new CharacterPart();
            m_PartDict.Add(partType, part);

            var partAvatar = avatarres.partList[partType];
            part.partType = partType;
            part.Init(this, partAvatar.prefab);
        }
    }

    public void ChangeAnim(AvatarRes avatarres)
    {
        if (m_Anim == null)
            return;

        //AnimationClip animclip = avatarres.mAnimList[avatarres.mAnimIdx];
        //m_Anim.wrapMode = WrapMode.Loop;
        //m_Anim.Play(animclip.name);
    }

    void DestroyAll()
    {
        if (m_Skeleton != null)
        {
            GameObject.DestroyImmediate(m_Skeleton);
        }
    }

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
