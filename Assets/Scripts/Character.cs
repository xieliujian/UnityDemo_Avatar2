using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public enum PartType
    {
        Eyes,
        Face,
        Hair,
        Pants,
        Shoes,
        Top,
    }

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

    GameObject m_Skeleton;
    GameObject m_Eyes;
    GameObject m_Face;
    GameObject m_Hair;
    GameObject m_Pants;
    GameObject m_Shoes;
    GameObject m_Top;
    Animation m_Anim;

    public void Generate(AvatarRes avatarres)
    {
        DestroyAll();

        m_Skeleton = GameObject.Instantiate(avatarres.mSkeleton);
        m_Skeleton.Reset(null);
        m_Skeleton.name = avatarres.mSkeleton.name;

        m_Anim = m_Skeleton.GetComponent<Animation>();

        ChangeEquip((int)PartType.Eyes, avatarres);
        ChangeEquip((int)PartType.Face, avatarres);
        ChangeEquip((int)PartType.Hair, avatarres);
        ChangeEquip((int)PartType.Pants, avatarres);
        ChangeEquip((int)PartType.Shoes, avatarres);
        ChangeEquip((int)PartType.Top, avatarres);

        ChangeAnim(avatarres);
    }

    public void ChangeEquip(int type, AvatarRes avatarres)
    {
        if (type == (int)PartType.Eyes)
        {
            ChangeEquip(ref m_Eyes, avatarres.mEyesList[avatarres.mEyesIdx]);
        }
        else if (type == (int)PartType.Face)
        {
            ChangeEquip(ref m_Face, avatarres.mFaceList[avatarres.mFaceIdx]);
        }
        else if (type == (int)PartType.Hair)
        {
            ChangeEquip(ref m_Hair, avatarres.mHairList[avatarres.mHairIdx]);
        }
        else if (type == (int)PartType.Pants)
        {
            ChangeEquip(ref m_Pants, avatarres.mPantsList[avatarres.mPantsIdx]);
        }
        else if (type == (int)PartType.Shoes)
        {
            ChangeEquip(ref m_Shoes, avatarres.mShoesList[avatarres.mShoesIdx]);
        }
        else if (type == (int)PartType.Top)
        {
            ChangeEquip(ref m_Top, avatarres.mTopList[avatarres.mTopIdx]);
        }
    }

    public void ChangeAnim(AvatarRes avatarres)
    {
        if (m_Anim == null)
            return;

        AnimationClip animclip = avatarres.mAnimList[avatarres.mAnimIdx];
        m_Anim.wrapMode = WrapMode.Loop;
        m_Anim.Play(animclip.name);
    }

    void DestroyAll()
    {
        if (m_Skeleton != null)
        {
            GameObject.DestroyImmediate(m_Skeleton);
        }

        m_Eyes = null;
        m_Face = null;
        m_Hair = null;
        m_Pants = null;
        m_Shoes = null;
        m_Top = null;
    }

    void ChangeEquip(ref GameObject go, GameObject resgo)
    {
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
        }

        go = GameObject.Instantiate(resgo);
        go.Reset(m_Skeleton);
        go.name = resgo.name;

        SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
        ShareSkeletonInstanceWith(render, m_Skeleton);
    }

    /// <summary>
    /// 共享骨骼
    /// </summary>
    /// <param name="selfSkin"></param>
    /// <param name="target"></param>
    void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    {
        Transform[] newBones = new Transform[selfSkin.bones.Length];
        for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
        {
            GameObject bone = selfSkin.bones[i].gameObject;
            if (bone == null)
                continue;
            
            // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
            newBones[i] = FunctionUtil.FindChildRecursion(target.transform, bone.name);
        }

        selfSkin.bones = newBones;
    }
}
