
#ifndef FACE_DECAL_INCLUDE
#define FACE_DECAL_INCLUDE

// Decal
half4 _FaceDecalColor;
half4 _FaceDecalUVMatrixM0;
half4 _FaceDecalUVMatrixM1;
half4 _FaceDecalUVMatrixM2;

half _FaceDecalScale;
half _FaceDecalOffsetX;
half _FaceDecalOffsetY;
half _FaceDecalRotation;

sampler2D _FaceDecalTex;

half4 CalcDecal(float2 uv)
{
    half3x3 calMatrix = half3x3(_FaceDecalUVMatrixM0.xyz,
        _FaceDecalUVMatrixM1.xyz,
        _FaceDecalUVMatrixM2.xyz);

    uv = saturate(mul(calMatrix, half3(uv, 1)).xy);
    half4 decalColor = tex2D(_FaceDecalTex, uv) * _FaceDecalColor;

    return decalColor;
}

#endif