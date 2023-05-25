Shader "Avatar2/Face"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        // Decal
        [HideInInspector] _FaceDecalTex("Decal Tex", 2D) = "black" {}
        [HideInInspector] _FaceDecalScale("Decal Scale", Range(0.01, 0.3)) = 0.1
        [HideInInspector] _FaceDecalOffsetX("Decal Offset X", Range(-0.5, 0.5)) = 0
        [HideInInspector] _FaceDecalOffsetY("Decal Offset Y", Range(-0.5, 0.5)) = 0
        [HideInInspector] _FaceDecalRotation("Decal Rotation", Float) = 0
        [HideInInspector] _FaceDecalColor("Face Decal Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _FaceDecalUVMatrixM0("_FaceDecalUVMatrixM0", Vector) = (1, 0, 0, 0)
        [HideInInspector] _FaceDecalUVMatrixM1("_FaceDecalUVMatrixM1", Vector) = (0, 1, 0, 0)
        [HideInInspector] _FaceDecalUVMatrixM2("_FaceDecalUVMatrixM2", Vector) = (0, 0, 1, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "FaceDecal.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                
                float4 decalCol = CalcDecal(i.uv);
                col.rgb = lerp(col.rgb, decalCol.rgb, decalCol.a);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            
            ENDCG
        }
    }

    CustomEditor "FaceShaderGUI"
}
