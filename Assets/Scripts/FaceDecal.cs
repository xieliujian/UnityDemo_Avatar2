using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceDecal
{
    static int s_FaceDecalUVMatrixM0ID = Shader.PropertyToID("_FaceDecalUVMatrixM0");
    static int s_FaceDecalUVMatrixM1ID = Shader.PropertyToID("_FaceDecalUVMatrixM1");
    static int s_FaceDecalUVMatrixM2ID = Shader.PropertyToID("_FaceDecalUVMatrixM2");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mat"></param>
    /// <returns></returns>
    public static void SetMat(Material material, Matrix4x4 matrix)
    {
        if (material == null)
            return;

        material.SetVector(s_FaceDecalUVMatrixM0ID, matrix.GetColumn(0));
        material.SetVector(s_FaceDecalUVMatrixM1ID, matrix.GetColumn(1));
        material.SetVector(s_FaceDecalUVMatrixM2ID, matrix.GetColumn(2));
    }

    /// <summary>
    /// .
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="translate"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Matrix4x4 TransformUV(float rotation, Vector2 translate, float scale)
    {
        Matrix4x4 matrix = Matrix4x4.identity;

        // rotate
        float radian = rotation * Mathf.Deg2Rad;
        float cosRadian = Mathf.Cos(radian);
        float sinRadian = Mathf.Sin(radian);
        Matrix4x4 rotateM = new Matrix4x4(new Vector4(cosRadian, sinRadian, 0, 0),
                                         new Vector4(-sinRadian, cosRadian, 0, 0),
                                         new Vector4(0, 0, 1, 0),
                                         new Vector4(0, 0, 0, 1));

        if (scale == 0)
        {
            scale = 1;
        }

        float invScale = 1 / scale;
        float offestX = -translate.x * invScale - 0.5f * invScale;
        float offsetY = -translate.y * invScale - 0.5f * invScale;

        // anchor translate
        Matrix4x4 preTranslateM = new Matrix4x4(new Vector4(1, 0, 0.5f, 0),
                                               new Vector4(0, 1, 0.5f, 0),
                                               new Vector4(0, 0, 1, 0),
                                               new Vector4(0, 0, 0, 1));

        // translate
        Matrix4x4 translateM = new Matrix4x4(new Vector4(1, 0, offestX, 0),
                                            new Vector4(0, 1, offsetY, 0),
                                            new Vector4(0, 0, 1, 0),
                                            new Vector4(0, 0, 0, 1));

        // scale
        Matrix4x4 scaleM = new Matrix4x4(new Vector4(invScale, 0, 0, 0),
                                        new Vector4(0, invScale, 0, 0),
                                        new Vector4(0, 0, 1, 0),
                                        new Vector4(0, 0, 0, 1));

        matrix = scaleM * translateM * rotateM * preTranslateM;

        return matrix;
    }
}
