using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PartBoneNamesHolder : ScriptableObject
{
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

    [SerializeField]
    List<Info> m_Infos = new List<Info>();

    public void Add(string partName, SkinnedMeshRenderer smr)
    {
        if (string.IsNullOrEmpty(partName) || smr == null)
        {
            return;
        }
        
        Info info = new Info();
        info.partName = partName;
        info.rootBoneName = smr.rootBone == null ? "" : smr.rootBone.name;

        List<string> boneNameList = new List<string>();
        foreach (var bone in smr.bones)
        {
            boneNameList.Add(bone.name);
        }

        info.boneNames = boneNameList.ToArray();

        info.bounds = smr.localBounds;

        info.trans = new TransInfo();
        info.trans.localPos = smr.gameObject.transform.localPosition;
        info.trans.localRot = smr.gameObject.transform.localRotation;
        info.trans.localScale = smr.gameObject.transform.localScale;

        m_Infos.Add(info);
    }

    public TransInfo GetTransInfo(string partName)
    {
        for (int i = 0; i < m_Infos.Count; ++i)
        {
            if (m_Infos[i].partName == partName)
            {
                return m_Infos[i].trans;
            }
        }

        return null;
    }

    public Bounds GetBounds(string partName)
    {
        for (int i = 0; i < m_Infos.Count; ++i)
        {
            if (m_Infos[i].partName == partName)
            {
                return m_Infos[i].bounds;
            }
        }

        return new Bounds();
    }
    
    public string[] GetBoneNames(string partName)
    {
        for (int i = 0; i < m_Infos.Count; ++i)
        {
            if (m_Infos[i].partName == partName)
            {
                return m_Infos[i].boneNames;
            }
        }

        return null;
    }

    public string GetBoneRootName(string partName)
    {
        for (int i = 0; i < m_Infos.Count; ++i)
        {
            if (m_Infos[i].partName == partName)
            {
                return m_Infos[i].rootBoneName;
            }
        }

        return string.Empty;
    }
}