using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class AvatarResPart
{
    public GameObject prefab;
}

public class AvatarRes
{
    public string mName;
    public PartBoneNamesHolder mBoneHolder;
    public List<AnimationClip> mAnimList = new List<AnimationClip>();
    public GameObject mSkeleton;
    public List<AvatarResPart> partList = new List<AvatarResPart>();

    public void Reset()
    {

    }

    public void AddAnimIdx()
    {

    }

    public void ReduceAnimIdx()
    {

    }

    public void AddIndex(int type)
    {

    }

    public void ReduceIndex(int type)
    {

    }
}

public class Main : MonoBehaviour
{
    #region 常量

    const int typeWidth = 240;
    const int typeheight = 100;
    const int buttonWidth = 60;

    #endregion

    #region 变量

    private List<AvatarRes> mAvatarResList = new List<AvatarRes>(); 
    private AvatarRes mAvatarRes = null;
    private int mAvatarResIdx = 0;

    private Character mCharacter = new Character();

    #endregion

    #region 内置函数

    // Use this for initialization
    void Start ()
    {
        CreateAllAvatarRes();
        InitCharacter();

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnGUI()
    {
        GUI.skin.box.fontSize = 50;
        GUI.skin.button.fontSize = 50;

        GUILayout.BeginArea(new Rect(10, 10, typeWidth + 2 * buttonWidth + 8, 1000));

        // Buttons for changing the active character.
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            ReduceAvatarRes();
            mCharacter.Generate(mAvatarRes);
        }

        GUILayout.Box("Character", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            AddAvatarRes();
            mCharacter.Generate(mAvatarRes);
        }

        GUILayout.EndHorizontal();

        // Buttons for changing character elements.
        AddCategory((int)CharacterPartType.Face, "Head", null);
        AddCategory((int)CharacterPartType.Eyes, "Eyes", null);
        AddCategory((int)CharacterPartType.Hair, "Hair", null);
        AddCategory((int)CharacterPartType.Top, "Body", "item_shirt");
        AddCategory((int)CharacterPartType.Pants, "Legs", "item_pants");
        AddCategory((int)CharacterPartType.Shoes, "Feet", "item_boots");

        // anim
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.ReduceAnimIdx();
            mCharacter.ChangeAnim(mAvatarRes);
        }

        GUILayout.Box("Anim", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddAnimIdx();
            mCharacter.ChangeAnim(mAvatarRes);
        }

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    // Draws buttons for configuring a specific category of items, like pants or shoes.
    void AddCategory(int parttype, string displayName, string anim)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.ReduceIndex(parttype);
            mCharacter.ChangeEquip(parttype, mAvatarRes);
        }

        GUILayout.Box(displayName, GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddIndex(parttype);
            mCharacter.ChangeEquip(parttype, mAvatarRes);
        }

        GUILayout.EndHorizontal();
    }

    #endregion

    #region 函数

    private void InitCharacter()
    {
        mAvatarRes = mAvatarResList[mAvatarResIdx];
        mCharacter.Generate(mAvatarRes);
    }

    private void CreateAllAvatarRes()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/");
        foreach(var subdir in dir.GetDirectories())
        {
            string[] splits = subdir.Name.Split('/');
            string dirname = splits[splits.Length - 1];

            GameObject [] golist = Resources.LoadAll<GameObject>(dirname);

            PartBoneNamesHolder[] boneHolder = Resources.LoadAll<PartBoneNamesHolder>(dirname);

            AvatarRes avatarres = new AvatarRes();
            mAvatarResList.Add(avatarres);

            avatarres.mName = dirname;
            avatarres.mSkeleton = FindRes(golist, Character.SKELETON_NAME)[0];
            avatarres.mBoneHolder = boneHolder[0];

            avatarres.partList.Clear();
            for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
            {
                var partName = Character.PART_NAMES[i];
                var partPrefab = FindRes(golist, partName)[0];

                AvatarResPart part = new AvatarResPart();
                part.prefab = partPrefab;
                avatarres.partList.Add(part);
            }

            //avatarres.mEyes = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Eyes])[0];
            //avatarres.mFace = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Face])[0];
            //avatarres.mHair = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Hair])[0];
            //avatarres.mPants = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Pants])[0];
            //avatarres.mShoes = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Shoes])[0];
            //avatarres.mTop = FindRes(golist, Character.PART_NAMES[(int)CharacterPartType.Top])[0];

            string animpath = "Assets/Anims/" + dirname + "/";
            List<AnimationClip> clips = FunctionUtil.CollectAll<AnimationClip>(animpath);
            avatarres.mAnimList.AddRange(clips);
        }
    }

    private List<GameObject> FindRes(GameObject []golist, string findname)
    {
        List<GameObject> findlist = new List<GameObject>();
        foreach (var go in golist)
        {
            if (go.name.Contains(findname))
            {
                findlist.Add(go);
            }
        }

        return findlist;
    }

    private void AddAvatarRes()
    {
        mAvatarResIdx++;
        if (mAvatarResIdx >= mAvatarResList.Count)
            mAvatarResIdx = 0;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }

    private void ReduceAvatarRes()
    {
        mAvatarResIdx--;
        if (mAvatarResIdx < 0)
            mAvatarResIdx = mAvatarResList.Count - 1;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }

    #endregion
}
