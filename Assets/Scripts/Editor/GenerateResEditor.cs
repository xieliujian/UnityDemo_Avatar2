using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class GenerateResEditor : Editor
{
    const string FBX_SUFFIX = ".fbx";
    const string ANIM_SUFFIX = ".anim";
    const string MESH_SUFFIX = ".mesh";
    const string PREAFAB_SUFFIX = ".prefab";
    const string INVALID_ANIM_NAME = "__preview__";
    const string ASSET_SUFFIX = ".asset";

    [MenuItem("Avatar2/Generate")]
    private static void Generate()
    {
        List<GameObject> objs = CollectObjs();

        if (objs.Count <= 0)
        {
            EditorUtility.DisplayDialog("Generator", "请选择UnuseRes文件夹", "Ok");
            return;
        }

        if (Directory.Exists(AvatarRes.ANIM_PATH))
            Directory.Delete(AvatarRes.ANIM_PATH, true);

        if (Directory.Exists(AvatarRes.Prefab_PATH))
            Directory.Delete(AvatarRes.Prefab_PATH, true);

        GenerateAllAnim(objs);
        GenerateAllMesh(objs);
        GenerateAllPrefab(objs);
        GenerateAllSkeleton(objs);
    }

    private static List<GameObject> CollectObjs()
    {
        List<GameObject> objs = new List<GameObject>();
        foreach (UnityEngine.Object o in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
        {
            GameObject obj = o as GameObject;
            if (!obj)
                continue;

            if (obj.name.Contains("@"))
                continue;

            objs.Add(obj);
        }

        return objs;
    }

    private static void GenerateAllSkeleton(List<GameObject> objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            dir = dir.Replace("\\", "/");
            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GenerateSkeleton(obj, dir, middir);
        }
    }

    private static void GenerateSkeleton(GameObject srcobj, string dir, string middir)
    {
        string prefabpath = AvatarRes.Prefab_PATH + "/" + middir + "/";
        string animpath = AvatarRes.ANIM_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        GameObject obj = GameObject.Instantiate(srcobj);
        obj.name = middir + Character.SKELETON_NAME;
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        foreach (SkinnedMeshRenderer smr in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            GameObject.DestroyImmediate(smr.gameObject);

        Animation anim = obj.GetComponent<Animation>();
        GameObject.DestroyImmediate(anim);

        anim = obj.AddComponent<Animation>();

        List<AnimationClip> clips = FunctionUtil.CollectAll<AnimationClip>(animpath);
        foreach (var clip in clips)
        {
            anim.AddClip(clip, clip.name);
        }

        string dstpath = prefabpath + obj.name.ToLower() + PREAFAB_SUFFIX;
        PrefabUtility.SaveAsPrefabAsset(obj, dstpath);

        GameObject.DestroyImmediate(obj);
    }

    private static void GenerateAllPrefab(List<GameObject> objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            dir = dir.Replace("\\", "/");
            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GeneratePrefab(obj, dir, middir);
        }
    }

    private static void GeneratePrefab(GameObject srcobj, string dir, string middir)
    {
        string prefabpath = AvatarRes.Prefab_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        dirinfo = new DirectoryInfo(prefabpath);
        if (!dirinfo.Exists)
        {
            Directory.CreateDirectory(prefabpath);
        }

        PartBoneNamesHolder partHolder = new PartBoneNamesHolder();

        List<string> partNameList = new List<string>(); 

        foreach (SkinnedMeshRenderer smr in srcobj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(smr.gameObject);
            SkinnedMeshRenderer renderer = obj.GetComponent<SkinnedMeshRenderer>();

            GameObject rendererParent = obj.transform.parent.gameObject;
            PrefabUtility.UnpackPrefabInstance(rendererParent, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (SkinnedMeshRenderer tempsmr in rendererParent.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (tempsmr != renderer)
                {
                    GameObject.DestroyImmediate(tempsmr.gameObject);
                }
            }

            Animation anim = rendererParent.GetComponent<Animation>();
            GameObject.DestroyImmediate(anim);

            var skinMesh = rendererParent.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinMesh != null)
            {
                var skinMeshName = skinMesh.name;

                partHolder.Add(skinMeshName, skinMesh);

                var isPrefabExist = IsPartPrefabExist(partNameList, skinMeshName, out string partName);
                if (isPrefabExist)
                {
                    GameObject newobj = GameObject.Instantiate(skinMesh.gameObject);
                    if (newobj != null)
                    {
                        newobj.name = partName;

                        var newSkinMesh = newobj.GetComponent<SkinnedMeshRenderer>();
                        if (newSkinMesh != null)
                        {
                            newSkinMesh.sharedMesh = null;
                            newSkinMesh.rootBone = null;

                            for (int i = 0; i < skinMesh.sharedMaterials.Length; i++)
                            {
                                newSkinMesh.sharedMaterials[i] = null;
                            }

                            newSkinMesh.sharedMaterial = null;
                        }

                        string dstpath = prefabpath + newobj.name + PREAFAB_SUFFIX;
                        PrefabUtility.SaveAsPrefabAsset(newobj, dstpath);

                        GameObject.DestroyImmediate(newobj);
                    }
                }
            }

            GameObject.DestroyImmediate(rendererParent);
        }

        var assetPath = prefabpath + Character.BONE_ASSET_NAME + ASSET_SUFFIX;
        AssetDatabase.CreateAsset(partHolder, assetPath);
    }

    /// <summary>
    /// 是否部件名字存在
    /// </summary>
    /// <param name="partName"></param>
    /// <returns></returns>
    static bool IsPartPrefabExist(List<string> partNameList, string skinMeshName, out string partName)
    {
        partName = "";

        foreach(var name in Character.PART_NAMES)
        {
            if (skinMeshName.StartsWith(name))
            {
                if (!partNameList.Contains(name))
                {
                    partName = name;
                    partNameList.Add(name);
                    return true;
                }
            }
        }

        return false;
    }

    static void GenerateAllMesh(List<GameObject> objs)
    {
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            dir = dir.Replace("\\", "/");
            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GenerateMesh(dir, middir, filename);
        }
    }

    static void GenerateAllAnim(List<GameObject> objs)
    {
        foreach(var obj in objs)
        {
            if (obj == null)
                continue;

            string path = AssetDatabase.GetAssetPath(obj);
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);

            dir = dir.Replace("\\", "/");
            string[] splitdirs = dir.Split('/');
            string middir = splitdirs[splitdirs.Length - 1];

            GenerateAnim(dir, middir, filename);
        }
    }

    static void GenerateMesh(string dir, string middir, string fbxname)
    {
        string meshPath = AvatarRes.MESH_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        dirinfo = new DirectoryInfo(meshPath);
        if (!dirinfo.Exists)
        {
            Directory.CreateDirectory(meshPath);
        }

        var files = Directory.GetFiles(dir, "*.fbx");
        foreach (var file in files)
        {
            if (file.Contains("@"))
                continue;

            UnityEngine.Object[] meshObjs = AssetDatabase.LoadAllAssetsAtPath(file);
            if (meshObjs.Length <= 0)
                continue;

            foreach (var meshObj in meshObjs)
            {
                Mesh srcMesh = meshObj as Mesh;
                if (srcMesh == null)
                    continue;

                if (srcMesh.name.Contains(INVALID_ANIM_NAME))
                    continue;

                string dstMeshPath = meshPath + srcMesh.name + MESH_SUFFIX;
                Mesh dstMesh = AssetDatabase.LoadAssetAtPath(dstMeshPath, typeof(Mesh)) as Mesh;
                if (dstMesh != null)
                {
                    AssetDatabase.DeleteAsset(dstMeshPath);
                }

                Mesh tempMesh = CopyMeshData(srcMesh);
                //EditorUtility.CopySerialized(srcMesh, tempMesh);

                AssetDatabase.CreateAsset(tempMesh, dstMeshPath);
            }
        }
    }

    /// <summary>
    /// 生成一个动画资源
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="middir"></param>
    /// <param name="fbxname"></param>
    static void GenerateAnim(string dir, string middir, string fbxname)
    {
        string clippath = AvatarRes.ANIM_PATH + "/" + middir + "/";

        DirectoryInfo dirinfo = new DirectoryInfo(dir);
        if (!dirinfo.Exists)
            return;

        dirinfo = new DirectoryInfo(clippath);
        if (!dirinfo.Exists)
        {
            Directory.CreateDirectory(clippath);
        }

        var files = Directory.GetFiles(dir, "*.fbx");
        foreach (var file in files)
        {
            if (!file.Contains("@"))
                continue;

            UnityEngine.Object[] clipobjs = AssetDatabase.LoadAllAssetsAtPath(file);
            if (clipobjs.Length <= 0)
                continue;

            foreach (var clipobj in clipobjs)
            {
                AnimationClip srcclip = clipobj as AnimationClip;
                if (srcclip == null)
                    continue;

                if (srcclip.name.Contains(INVALID_ANIM_NAME))
                    continue;

                string dstclippath = clippath + srcclip.name + ANIM_SUFFIX;
                AnimationClip dstclip = AssetDatabase.LoadAssetAtPath(dstclippath, typeof(AnimationClip)) as AnimationClip;
                if (dstclip != null)
                    AssetDatabase.DeleteAsset(dstclippath);

                AnimationClip tempclip = new AnimationClip();
                EditorUtility.CopySerialized(srcclip, tempclip);
                AssetDatabase.CreateAsset(tempclip, dstclippath);
            }
        }
    }

    static int[] GetNewVertexIndexes(Vector3[] vertices, Vector2[] uv, out int newVertexCount)
    {
        int vertexCount = vertices.Length;
        int[] vertexIndexs = new int[vertexCount];

        float posDiff = 0.001f;
        float posSQDiff = posDiff * posDiff;

        float uvDiff = 0.1f;
        float uvSQDiff = uvDiff * uvDiff;

        for (int i = 0; i < vertexCount; ++i)
        {
            vertexIndexs[i] = i;
        }

        for (int i = 0; i < vertexCount; ++i)
        {
            // 顶点已经被重定向了，不需要再次计算
            if (vertexIndexs[i] != i)
            {
                continue;
            }

            for (int j = i + 1; j < vertexCount; ++j)
            {
                Vector3 v0 = vertices[i];
                Vector3 v1 = vertices[j];

                Vector2 uv0 = uv[i];
                Vector2 uv1 = uv[j];

                if ((v0.x - v1.x) * (v0.x - v1.x) + (v0.y - v1.y) * (v0.y - v1.y) + (v0.z - v1.z) * (v0.z - v1.z) <= posSQDiff)
                {
                    //if ((uv0.x - uv1.x) * (uv0.x - uv1.x) + (uv0.y - uv1.y) * (uv0.y - uv1.y) <= uvSQDiff)
                    {
                        vertexIndexs[j] = i;
                    }
                }
            }
        }

        newVertexCount = 0;

        for (int i = 0; i < vertexCount; ++i)
        {
            if (vertexIndexs[i] == i)
            {
                ++newVertexCount;
            }
        }

        return vertexIndexs;
    }

    static Mesh CopyMeshData(Mesh mesh)
    {
        int vertexCount = mesh.vertexCount;
        int subMeshCount = mesh.subMeshCount;
        var subMeshTriangles = new int[subMeshCount][];
        for (int i = 0; i < subMeshCount; i++)
        {
            subMeshTriangles[i] = mesh.GetTriangles(i);
        }

        // old data
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector4[] tangents = mesh.tangents;

        Vector2[] uv = mesh.uv;
        Vector2[] uv2 = mesh.uv2;
        Vector2[] uv3 = mesh.uv3;
        Vector2[] uv4 = mesh.uv4;
        Vector2[] uv5 = mesh.uv5;
        Vector2[] uv6 = mesh.uv6;
        Vector2[] uv7 = mesh.uv7;
        Vector2[] uv8 = mesh.uv8;

        Color[] colors = mesh.colors;
        BoneWeight[] boneWeights = mesh.boneWeights;
        Matrix4x4[] bindposes = mesh.bindposes;   

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.normals = normals;
        newMesh.tangents = tangents;
        newMesh.uv = uv;
        newMesh.uv2 = uv2;
        newMesh.uv3 = uv3;
        newMesh.uv4 = uv4;
        newMesh.uv5 = uv5;
        newMesh.uv6 = uv6;
        newMesh.uv7 = uv7;
        newMesh.uv8 = uv8;
        newMesh.colors = colors;
        newMesh.boneWeights = boneWeights;
        newMesh.bindposes = bindposes;

        newMesh.subMeshCount = subMeshTriangles.Length;

        for (int i = 0; i < subMeshTriangles.Length; ++i)
        {
            newMesh.SetTriangles(subMeshTriangles[i], i);
        }

        return newMesh;
    }

    static Mesh CopyMeshDataOptimize(Mesh mesh)
    {
        int vertexCount = mesh.vertexCount;
        int subMeshCount = mesh.subMeshCount;
        var subMeshTriangles = new int[subMeshCount][];
        for (int i = 0; i < subMeshCount; i++)
        {
            subMeshTriangles[i] = mesh.GetTriangles(i);
        }

        // old data
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector4[] tangents = mesh.tangents;

        Vector2[] uv = mesh.uv;
        Vector2[] uv2 = mesh.uv2;
        Vector2[] uv3 = mesh.uv3;
        Vector2[] uv4 = mesh.uv4;
        Vector2[] uv5 = mesh.uv5;
        Vector2[] uv6 = mesh.uv6;
        Vector2[] uv7 = mesh.uv7;
        Vector2[] uv8 = mesh.uv8;

        Color[] colors = mesh.colors;
        BoneWeight[] boneWeights = mesh.boneWeights;
        Matrix4x4[] bindposes = mesh.bindposes;


        // new data
        int newVertexCount = 0;
        int[] newVertexIndexes = GetNewVertexIndexes(vertices, uv, out newVertexCount);

        Vector3[] newVertices = new Vector3[newVertexCount];
        Vector3[] newNormals = new Vector3[normals.Length == 0 ? 0 : newVertexCount];
        Vector4[] newTangents = new Vector4[tangents.Length == 0 ? 0 : newVertexCount];

        Vector2[] newUv = new Vector2[uv.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv2 = new Vector2[uv2.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv3 = new Vector2[uv3.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv4 = new Vector2[uv4.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv5 = new Vector2[uv5.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv6 = new Vector2[uv6.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv7 = new Vector2[uv7.Length == 0 ? 0 : newVertexCount];
        Vector2[] newUv8 = new Vector2[uv8.Length == 0 ? 0 : newVertexCount];

        Color[] newColors = new Color[colors.Length == 0 ? 0 : newVertexCount];
        BoneWeight[] newBoneWeights = new BoneWeight[boneWeights.Length == 0 ? 0 : newVertexCount];

        int pos = 0;
        int[] vertexRedirectIndexes = new int[vertexCount];
        for (int i = 0; i < vertexCount; ++i)
        {
            int newIndex = newVertexIndexes[i];
            vertexRedirectIndexes[i] = -1;

            // 重定向
            if (newIndex != i)
            {
                continue;
            }

            vertexRedirectIndexes[i] = pos;

            // 没有重定向，直接拷贝
            newVertices[pos] = vertices[i];

            if (newNormals.Length != 0)
            {
                newNormals[pos] = normals[i];
            }

            if (newTangents.Length != 0)
            {
                newTangents[pos] = tangents[i];
            }

            if (newUv.Length != 0)
            {
                newUv[pos] = uv[i];
            }

            if (newUv2.Length != 0)
            {
                newUv2[pos] = uv2[i];
            }

            if (newUv3.Length != 0)
            {
                newUv3[pos] = uv3[i];
            }

            if (newUv4.Length != 0)
            {
                newUv4[pos] = uv4[i];
            }

            if (newUv5.Length != 0)
            {
                newUv5[pos] = uv5[i];
            }

            if (newUv6.Length != 0)
            {
                newUv6[pos] = uv6[i];
            }

            if (newUv7.Length != 0)
            {
                newUv7[pos] = uv7[i];
            }

            if (newUv8.Length != 0)
            {
                newUv8[pos] = uv8[i];
            }

            if (newColors.Length != 0)
            {
                newColors[pos] = colors[i];
            }

            if (newBoneWeights.Length != 0)
            {
                newBoneWeights[pos] = boneWeights[i];
            }

            ++pos;
        }

        for (int i = 0; i < vertexCount; ++i)
        {
            if (vertexRedirectIndexes[i] == -1)
            {
                int newIndex = newVertexIndexes[i];
                vertexRedirectIndexes[i] = vertexRedirectIndexes[newIndex];
            }
        }

        foreach (var subMeshTriangle in subMeshTriangles)
        {
            for (int i = 0; i < subMeshTriangle.Length; ++i)
            {
                int v = subMeshTriangle[i];
                subMeshTriangle[i] = vertexRedirectIndexes[v];
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = newVertices;
        newMesh.normals = newNormals;
        newMesh.tangents = newTangents;
        newMesh.uv = newUv;
        newMesh.uv2 = newUv2;
        newMesh.uv3 = newUv3;
        newMesh.uv4 = newUv4;
        newMesh.uv5 = newUv5;
        newMesh.uv6 = newUv6;
        newMesh.uv7 = newUv7;
        newMesh.uv8 = newUv8;
        newMesh.colors = newColors;
        newMesh.boneWeights = newBoneWeights;
        newMesh.bindposes = bindposes;

        newMesh.subMeshCount = subMeshTriangles.Length;

        for (int i = 0; i < subMeshTriangles.Length; ++i)
        {
            newMesh.SetTriangles(subMeshTriangles[i], i);
        }

        return newMesh;
    }
}
