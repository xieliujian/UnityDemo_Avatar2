using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region 变量

    private GameObject mSkeleton;
    private GameObject mEyes;
    private GameObject mFace;
    private GameObject mHair;
    private GameObject mPants;
    private GameObject mShoes;
    private GameObject mTop;
    private Animation mAnim;

    #endregion

    #region 内置函数

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    #endregion

    #region 函数

    public void SetName(string name)
    {
        gameObject.name = name;
    }

    public void Generate(AvatarRes avatarres)
    {
        GenerateUnCombine(avatarres);
    }

    private void DestroyAll()
    {
        if (mSkeleton != null)
            GameObject.DestroyImmediate(mSkeleton);

        mEyes = null;
        mFace = null;
        mHair = null;
        mPants = null;
        mShoes = null;
        mTop = null;
    }

    private void GenerateUnCombine(AvatarRes avatarres)
    {
        DestroyAll();

        mSkeleton = GameObject.Instantiate(avatarres.mSkeleton);
        mSkeleton.Reset(gameObject);
        mSkeleton.name = avatarres.mSkeleton.name;

        mAnim = mSkeleton.GetComponent<Animation>();

        ChangeEquipUnCombine((int)EPart.EP_Eyes, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Face, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Hair, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Pants, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Shoes, avatarres);
        ChangeEquipUnCombine((int)EPart.EP_Top, avatarres);

        ChangeAnim(avatarres);
    }

    public void ChangeEquipUnCombine(int type, AvatarRes avatarres)
    {
        if (type == (int)EPart.EP_Eyes)
        {
            ChangeEquipUnCombine(ref mEyes, avatarres.mEyesList[avatarres.mEyesIdx]);
        }
        else if (type == (int)EPart.EP_Face)
        {
            ChangeEquipUnCombine(ref mFace, avatarres.mFaceList[avatarres.mFaceIdx]);
        }
        else if (type == (int)EPart.EP_Hair)
        {
            ChangeEquipUnCombine(ref mHair, avatarres.mHairList[avatarres.mHairIdx]);
        }
        else if (type == (int)EPart.EP_Pants)
        {
            ChangeEquipUnCombine(ref mPants, avatarres.mPantsList[avatarres.mPantsIdx]);
        }
        else if (type == (int)EPart.EP_Shoes)
        {
            ChangeEquipUnCombine(ref mShoes, avatarres.mShoesList[avatarres.mShoesIdx]);
        }
        else if (type == (int)EPart.EP_Top)
        {
            ChangeEquipUnCombine(ref mTop, avatarres.mTopList[avatarres.mTopIdx]);
        }
    }

    private void ChangeEquipUnCombine(ref GameObject go, GameObject resgo)
    {
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
        }

        go = GameObject.Instantiate(resgo);
        go.Reset(mSkeleton);
        go.name = resgo.name;

        SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
        ShareSkeletonInstanceWith(render, mSkeleton);
    }

    // 共享骨骼
    public void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
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

    public void ChangeAnim(AvatarRes avatarres)
    {
        if (mAnim == null)
            return;

        AnimationClip animclip = avatarres.mAnimList[avatarres.mAnimIdx];
        mAnim.wrapMode = WrapMode.Loop;
        mAnim.Play(animclip.name);
    }

    #endregion
}
