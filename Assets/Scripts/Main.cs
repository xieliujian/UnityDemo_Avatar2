using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarResPart
{
    public string partName;

    public GameObject prefab;

    public List<Mesh> meshList = new List<Mesh>();

    List<List<Material>> matList = new List<List<Material>>();

    public int meshIdx;

    public int matIdx;

    public void FillMesh(List<Mesh> meshArray)
    {
        if (meshArray == null)
            return;

        meshList.Clear();
        foreach (var mesh in meshArray)
        {
            if (mesh == null)
                continue;

            if (mesh.name.Contains(partName))
            {
                meshList.Add(mesh);
            }
        }

        matList.Clear();
        for (int i = 0; i < meshList.Count; i++)
        {
            matList.Add(new List<Material>());
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

            var list = FindMatInternalList(mat);
            if (list != null)
            {
                list.Add(mat);
            }
        }
    }

    public Material FindMat()
    {
        if (matList.Count == 0 || meshIdx >= matList.Count) return null;
        var list = matList[meshIdx];
        if (list.Count == 0 || matIdx >= list.Count) return null;
        return list[matIdx];
    }

    public Mesh FindMesh()
    {
        if (meshList.Count == 0 || meshIdx >= meshList.Count) return null;
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
        matIdx = 0;

        if (meshIdx >= meshList.Count)
        {
            meshIdx = 0;
        }
    }

    public void ReduceMeshIndex()
    {
        meshIdx--;
        matIdx = 0;

        if (meshIdx < 0)
        {
            meshIdx = meshList.Count - 1;
        }
    }

    public void AddMatIndex()
    {
        matIdx++;

        if (matIdx >= matList[meshIdx].Count)
        {
            matIdx = 0;
        }
    }

    public void ReduceMatIndex()
    {
        matIdx--;

        if (matIdx < 0)
        {
            matIdx = matList[meshIdx].Count - 1;
        }
    }

    List<Material> FindMatInternalList(Material mat)
    {
        for (int i = 0; i < meshList.Count; i++)
        {
            var mesh = meshList[i];
            if (mesh == null)
                continue;

            if (mat.name.Contains(mesh.name))
            {
                return matList[i];
            }
        }

        return null;
    }
}

public class AvatarRes
{
    // 编辑器生成工具使用的根目录（System.IO 路径）
    public const string Prefab_PATH = "Assets/Resources/";

    // Resources.LoadAll 的子路径（相对于 Assets/Resources/<CharName>/）
    public const string MESH_SUBPATH = "Meshs";
    public const string ANIM_SUBPATH = "Anims";
    public const string MAT_SUBPATH = "Materials";

    // 已知的角色目录名，替代运行时扫描文件系统（WebGL 无文件系统访问）
    public static readonly string[] CHAR_NAMES = { "Female", "Male" };

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
    // 参考分辨率，所有 GUI 尺寸基于此设计
    const float REF_WIDTH  = 1920f;
    const float REF_HEIGHT = 1080f;

    const int typeWidth   = 240;
    const int typeHeight  = 100;
    const int buttonWidth = 60;
    const int fontSize    = 50;

    [Header("摄像机控制器（可选）")]
    public CameraController cameraController;

    private List<AvatarRes> mAvatarResList = new List<AvatarRes>(); 
    private AvatarRes mAvatarRes = null;
    private int mAvatarResIdx = 0;

    private Character mCharacter = new Character();

    void Start()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        CreateAllAvatarRes();
        InitCharacter();
    }

    void Update() { }

    void OnGUI()
    {
        // 按参考分辨率等比缩放，保持宽高比，居左上对齐
        float scale = Mathf.Min(Screen.width / REF_WIDTH, Screen.height / REF_HEIGHT);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        GUI.skin.box.fontSize    = fontSize;
        GUI.skin.button.fontSize = fontSize;

        int areaWidth = typeWidth + 2 * buttonWidth + 8;
        GUILayout.BeginArea(new Rect(10, 10, areaWidth, REF_HEIGHT - 20));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            ReduceAvatarRes();
            Generate(mAvatarRes);
            cameraController?.ResetAngle();
        }
        GUILayout.Box("Character", GUILayout.Width(typeWidth), GUILayout.Height(typeHeight));
        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            AddAvatarRes();
            Generate(mAvatarRes);
            cameraController?.ResetAngle();
        }
        GUILayout.EndHorizontal();

        AddCategory((int)CharacterPartType.Face,  "Head", null);
        AddCategory((int)CharacterPartType.Eyes,  "Eyes", null);
        AddCategory((int)CharacterPartType.Hair,  "Hair", null);
        AddCategory((int)CharacterPartType.Top,   "Body", "item_shirt");
        AddCategory((int)CharacterPartType.Pants, "Legs", "item_pants");
        AddCategory((int)CharacterPartType.Shoes, "Feet", "item_boots");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            mAvatarRes.ReduceAnimIdx();
            ChangeAnim(mAvatarRes);
        }
        GUILayout.Box("Anim", GUILayout.Width(typeWidth), GUILayout.Height(typeHeight));
        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            mAvatarRes.AddAnimIdx();
            ChangeAnim(mAvatarRes);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    void AddCategory(int parttype, string displayName, string anim)
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            mAvatarRes.ReduceMeshIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }
        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            mAvatarRes.AddMeshIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }

        GUILayout.Box(displayName, GUILayout.Width(typeWidth), GUILayout.Height(typeHeight));

        if (GUILayout.Button("<", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
        {
            mAvatarRes.ReduceMatIndex(parttype);
            ChangeEquip(parttype, mAvatarRes);
        }
        if (GUILayout.Button(">", GUILayout.Width(buttonWidth), GUILayout.Height(typeHeight)))
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

        // 骨骼加载完成后立即绑定摄像机目标，部件加载失败也不影响摄像机跟随
        if (cameraController != null && mCharacter.skeleton != null)
            cameraController.target = mCharacter.skeleton.transform;

        for (int i = 0; i < (int)CharacterPartType.TotalNum; i++)
        {
            var part = res.partList[i];
            if (part == null)
                continue;

            var mesh = part.FindMesh();
            var mat = part.FindMat();

            if (mesh == null || mat == null)
            {
                Debug.LogWarning($"[Generate] 部件 {part.partName} 的 Mesh 或 Material 未加载，" +
                    $"请确认资源已放在 Assets/Resources/{res.mName}/{AvatarRes.MESH_SUBPATH}/ 和 {AvatarRes.MAT_SUBPATH}/ 下");
                continue;
            }

            mCharacter.InitPart(i, part.prefab);

            List<Material> matList = new List<Material>();
            matList.Add(mat);

            mCharacter.ChangePart(i, mesh, matList);
        }

        ChangeAnim(res);
    }

    void CreateAllAvatarRes()
    {
        // 使用已知角色名列表，避免在 WebGL/运行时使用 System.IO 扫描文件系统
        foreach (string dirname in AvatarRes.CHAR_NAMES)
        {
            GameObject[] golist = Resources.LoadAll<GameObject>(dirname);
            PartBoneNamesHolder[] boneHolder = Resources.LoadAll<PartBoneNamesHolder>(dirname);

            if (golist == null || golist.Length == 0 || boneHolder == null || boneHolder.Length == 0)
            {
                Debug.LogWarning($"[CreateAllAvatarRes] 跳过 {dirname}，Resources 下未找到所需资源");
                continue;
            }

            AvatarRes avatarres = new AvatarRes();
            mAvatarResList.Add(avatarres);

            avatarres.mName = dirname;
            avatarres.mSkeleton = FindRes(golist, Character.SKELETON_NAME)[0];
            avatarres.mBoneHolder = boneHolder[0];

            // Mesh / Material / AnimationClip 须放在 Assets/Resources/<dirname>/Meshs|Materials|Anims/ 下
            // Resources.LoadAll 在所有平台（含 WebGL）均可用
            string meshResPath = $"{dirname}/{AvatarRes.MESH_SUBPATH}";
            List<Mesh> meshArray = new List<Mesh>(Resources.LoadAll<Mesh>(meshResPath));

            string matResPath = $"{dirname}/{AvatarRes.MAT_SUBPATH}";
            List<Material> matArray = new List<Material>(Resources.LoadAll<Material>(matResPath));

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

            string animResPath = $"{dirname}/{AvatarRes.ANIM_SUBPATH}";
            List<AnimationClip> clips = new List<AnimationClip>(Resources.LoadAll<AnimationClip>(animResPath));
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

        List<Material> matList = new List<Material>();
        matList.Add(mat);

        mCharacter.ChangePart(parttype, mesh, matList);
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
