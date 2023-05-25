using System;
using UnityEditor;
using UnityEngine;

public class FaceShaderGUI : ShaderGUI
{
    MaterialProperty m_FaceDecalTex;
    MaterialProperty m_FaceDecalScale;
    MaterialProperty m_FaceDecalOffsetX;
    MaterialProperty m_FaceDecalOffsetY;
    MaterialProperty m_FaceDecalRotation;
    MaterialProperty m_FaceDecalColor;

    bool m_Init;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        if (!m_Init)
        {
            InitProperty(properties);
            m_Init = true;
        }

        DrawDecal(materialEditor);
    }

    void InitProperty(MaterialProperty[] properties)
    {
        m_FaceDecalTex = FindProperty("_FaceDecalTex", properties, false);
        m_FaceDecalScale = FindProperty("_FaceDecalScale", properties, false);
        m_FaceDecalOffsetX = FindProperty("_FaceDecalOffsetX", properties, false);
        m_FaceDecalOffsetY = FindProperty("_FaceDecalOffsetY", properties, false);
        m_FaceDecalRotation = FindProperty("_FaceDecalRotation", properties, false);
        m_FaceDecalColor = FindProperty("_FaceDecalColor", properties, false);
    }

    void DrawDecal(MaterialEditor editor)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Decal", EditorStyles.boldLabel);

        editor.TextureProperty(m_FaceDecalTex, "Decal Tex", false);

        Material material = editor.target as Material;

        EditorGUI.BeginChangeCheck();
        m_FaceDecalScale.floatValue = EditorGUILayout.Slider("Decal Scale", m_FaceDecalScale.floatValue, 0.01f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            SetUV(material);
        }

        EditorGUI.BeginChangeCheck();
        m_FaceDecalOffsetX.floatValue = EditorGUILayout.FloatField("Decal Offset X", m_FaceDecalOffsetX.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            SetUV(material);
        }
        EditorGUI.BeginChangeCheck();
        m_FaceDecalOffsetY.floatValue = EditorGUILayout.FloatField("Decal Offset Y", m_FaceDecalOffsetY.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            SetUV(material);
        }

        EditorGUI.BeginChangeCheck();
        m_FaceDecalRotation.floatValue = EditorGUILayout.FloatField("Decal Rotation", m_FaceDecalRotation.floatValue);
        if (EditorGUI.EndChangeCheck())
        {
            SetUV(material);
        }

        EditorGUI.BeginChangeCheck();
        m_FaceDecalColor.colorValue = EditorGUILayout.ColorField("Decal Color", m_FaceDecalColor.colorValue);
        if (EditorGUI.EndChangeCheck())
        {
            SetUV(material);
        }
    }

    void SetUV(Material material)
    {
        float rotation = m_FaceDecalRotation.floatValue;
        float scale = m_FaceDecalScale.floatValue;
        Vector2 offset = new Vector2(m_FaceDecalOffsetX.floatValue, m_FaceDecalOffsetY.floatValue);
        Matrix4x4 matrix = FaceDecal.TransformUV(rotation, offset, scale);
        FaceDecal.SetMat(material, matrix);
    }
}
