using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PartBoneNamesHolder : ScriptableObject
{
    [Serializable]
    public struct Info
    {
        public string partName;
        public string rootBoneName;
        public string[] boneNames;
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
        m_Infos.Add(info);
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