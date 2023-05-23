using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class AvatarResPart
{
    public string partName;

    public GameObject prefab;

    public List<Mesh> meshList = new List<Mesh>();

    public List<Material> matList = new List<Material>();

    public int meshIdx;

    public int matIdx;

    public void FillMesh(List<Mesh> meshArray)
    {
        if (meshArray == null)
            return;

        foreach(var mesh in meshArray)
        {
            if (mesh == null)
                continue;

            if (mesh.name.Contains(partName))
            {
                meshList.Add(mesh);
            }
        }
    }

    public void FillMat(List<Material> matArray)
    {
        if (matArray == null)
            return;

        foreach(var mat in matArray)
        {
            if (mat == null)
                continue;

            if (mat.name.Contains(partName))
            {
                matList.Add(mat);
            }
        }
    }

    public Material FindMat()
    {
        return matList[matIdx];
    }

    public Mesh FindMesh()
    {
        return meshList[meshIdx];
    }

    public void ResetIndex()
    {
        meshIdx = 0;
        matIdx = 0;
    }

    public void AddMeshIndex()
    {
        meshIdx++;

        if (meshIdx >= meshList.Count)
        {
            meshIdx = 0;
        }
    }

    public void ReduceMeshIndex()
    {
        meshIdx--;

        if (meshIdx < 0)
        {
            meshIdx = meshList.Count - 1;
        }
    }

    public void AddMatIndex()
    {
        matIdx++;

        if (matIdx >= matList.Count)
        {
            matIdx = 0;
        }
    }

    public void ReduceMatIndex()
    {
        matIdx--;

        if (matIdx < 0)
        {
            matIdx = matList.Count - 1;
        }
    }
}

public class AvatarRes
{
    public const string MESH_PATH = "Assets/Meshs/";
    public const string ANIM_PATH = "Assets/Anims/";
    public const string Prefab_PATH = "Assets/Resources/";
    public const string MAT_PATH = "Assets/Materials/";

    public string mName;
    public PartBoneNamesHolder mBoneHolder;
    public List<AnimationClip> mAnimList = new List<AnimationClip>();
    public GameObject mSkeleton;
    public List<AvatarResPart> partList = new List<AvatarResPart>();

    public int mAnimIdx;

    public void Reset()
    {
        mAnimIdx = 0;  
    }

    public void AddAnimIdx()
    {
        mAnimIdx++;

        if (mAnimIdx >= mAnimList.Count)
        {
            mAnimIdx = 0;
        }
    }

    public void ReduceAnimIdx()
    {
        mAnimIdx--;

        if (mAnimIdx < 0)
        {
            mAnimIdx = mAnimList.Count - 1;
        }
    }

    public void AddMeshIndex(int type)
    {
        var part = partList[type];
        if (part != null)
        {
            part.AddMeshIndex();
        }
    }

    public void ReduceMeshIndex(int type)
    {
        var part = partList[type];
        if (part != null)
        {
            part.ReduceMeshIndex();
        }
    }

    public void AddMatIndex(int type)
    {
        var part = partList[type];
        if (part != null)
        {
            part.AddMatIndex();
        }
    }

    public void ReduceMatIndex(int type)
    {
        var part = partList[type];
        if (part != null)
        {
            part.ReduceMatIndex();
        }
    }
}

public class Main : MonoBehaviour
{
    const int typeWidth = 240;
    const int typeheight = 100;
    const int buttonWidth = 60;

    private List<AvatarRes> mAvatarResList = new List<AvatarRes>(); 
    private AvatarRes mAvatarRes = null;
    private int mAvatarResIdx = 0;

    private Character mCharacter = new Character();

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

    void OnGUI()
    {
        GUI.skin.box.fontSize = 50;
        GUI.skin.button.fontSize = 50;

        GUILayout.BeginArea(new Rect(10, 10, typeWidth + 150 + 2 * buttonWidth + 8, 1000));

        // Buttons for changing the active character.
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            ReduceAvatarRes();
            Generate(mAvatarRes);
        }

        GUILayout.Box("Character", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            AddAvatarRes();
            Generate(mAvatarRes);
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
            ChangeAnim(mAvatarRes);
        }

        GUILayout.Box("Anim", GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddAnimIdx();
            ChangeAnim(mAvatarRes);
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
            mAvatarRes.ReduceMeshIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddMeshIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }

        GUILayout.Box(displayName, GUILayout.Width(typeWidth), GUILayout.Height(typeheight));

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.ReduceMatIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }

        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeheight)))
        {
            mAvatarRes.AddMatIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }

        GUILayout.EndHorizontal();
    }

    void InitCharacter()
    {
        mAvatarRes = mAvatarResList[mAvatarResIdx];
        Generate(mAvatarRes);
    }

    void Generate(AvatarRes res)
    {
        if (res == null)
            return;

        mCharacter.charName = res.mName;
        mCharacter.InitSkeleton(res.mSkeleton);
        mCharacter.InitPartAsset(res.mBoneHolder);

        for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
        {
            var part = res.partList[i];
            if (part == null)
                continue;

            mCharacter.InitPart(i, part.prefab);

            var mesh = part.FindMesh();
            var mat = part.FindMat();
            mCharacter.ChangePart(i, mesh, mat);
        }

        ChangeAnim(res);
    }

    void CreateAllAvatarRes()
    {
        DirectoryInfo dir = new DirectoryInfo(AvatarRes.Prefab_PATH);
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

            string meshPath = AvatarRes.MESH_PATH + dirname + "/";
            List<Mesh> meshArray = FunctionUtil.CollectAll<Mesh>(meshPath);

            string matPath = AvatarRes.MAT_PATH + dirname + "/";
            List<Material> matArray = FunctionUtil.CollectAll<Material>(matPath);

            avatarres.partList.Clear();
            for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
            {
                var partName = Character.PART_NAMES[i];
                var partPrefab = FindRes(golist, partName)[0];

                AvatarResPart part = new AvatarResPart();
                part.partName = Character.PART_NAMES[i];
                part.prefab = partPrefab;
                part.FillMesh(meshArray);
                part.FillMat(matArray);
                avatarres.partList.Add(part);
            }

            string animpath = AvatarRes.ANIM_PATH + dirname + "/";
            List<AnimationClip> clips = FunctionUtil.CollectAll<AnimationClip>(animpath);
            avatarres.mAnimList.AddRange(clips);
        }
    }

    void ChangeEquip(int parttype, AvatarRes res)
    {
        var part = mAvatarRes.partList[parttype];
        if (part == null)
            return;

        var mesh = part.FindMesh();
        var mat = part.FindMat();
        mCharacter.ChangePart(parttype, mesh, mat);
    }

    void ChangeAnim(AvatarRes res)
    {
        AnimationClip animclip = res.mAnimList[res.mAnimIdx];
        mCharacter.ChangeAnim(animclip);
    }

    List<GameObject> FindRes(GameObject []golist, string findname)
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

    void AddAvatarRes()
    {
        mAvatarResIdx++;
        if (mAvatarResIdx >= mAvatarResList.Count)
            mAvatarResIdx = 0;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }

    void ReduceAvatarRes()
    {
        mAvatarResIdx--;
        if (mAvatarResIdx < 0)
            mAvatarResIdx = mAvatarResList.Count - 1;

        mAvatarRes = mAvatarResList[mAvatarResIdx];
    }
}
