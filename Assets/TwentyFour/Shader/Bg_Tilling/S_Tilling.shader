Shader "Unlit/MineCustom/Optimized_Tilling"
{
    Properties
    {
        _MainTex ("材质", 2D) = "white" {}
        _MaskTex ("遮罩蒙版（白色为可见）", 2D) = "white" {}
        _Color("控制纹理贴图的颜色", Color) = (1,1,1,1)  
        _Tiling("控制纹理重复的次数", Float) = 1
        _Rotation ("旋转角度", Range(0, 360)) = 0
        _OffsetSpeed("位移速度", Float) = 0.1
        _OffsetAngle("位移角度", Range(0, 360)) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Rotation; 
            float _Tiling;
            float _OffsetSpeed;
            float _OffsetAngle;

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 uvWorld : TEXCOORD1;
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                //预计算旋转和位移参数
                float time = _Time.y * _OffsetSpeed * 0.1f;
                float offsetRad = _OffsetAngle * UNITY_PI / 180.0;
                float2 offsetUV = float2(cos(offsetRad), sin(offsetRad)) * time;

                float rad = _Rotation * UNITY_PI / 180.0;
                //在顶点着色器中，旋转角度和位移角度的弧度计算是固定的，提前计算好 cosRad 和 sinRad，避免在每次计算中重复调用 cos 和 sin 函数。
                float cosRad = cos(rad);
                float sinRad = sin(rad);

                //应用位移和旋转
                float2 uv = v.uv + offsetUV;
                float2 uvCentered = uv - 0.5;
                float2 rotatedUV = float2(uvCentered.x * cosRad - uvCentered.y * sinRad, 
                                          uvCentered.x * sinRad + uvCentered.y * cosRad) + 0.5;

                //应用平铺
                o.uv = rotatedUV * _Tiling;
                o.uvWorld = v.uv; //遮罩UV保持不变
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color; //合并采样和颜色控制
                fixed4 maskColor = tex2D(_MaskTex, i.uvWorld);
                return fixed4(texColor.rgb, texColor.a * maskColor.r); //使用遮罩控制透明度
            }
            ENDCG
        }
    }
}