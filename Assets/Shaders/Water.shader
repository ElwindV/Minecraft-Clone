Shader "Unlit/Water"
{
    Properties
    {        
        _MainTex ("Texture", 2D) = "white" {}
        
        // Depth
        _ShallowColor ("Shallow Color", Color) = (0.01171875, 0.66015625, 0.953125, 0.7)
        _DeepColor ("Deep", Color) = (0.24609375, 0.31640625, 0.70703125, 0.9)
        _Distance ("Distance", Range(0, 5)) = 1
        
        // Foam
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDist ("Foam Distance", Range(0, 1)) = 1
        
        // Wave Displacement
        _WavesTex ("Wave Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _DepthMultiplier;
            
            sampler2D _WavesTex;
            float4 _WavesTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                o.screenPos = ComputeScreenPos(o.vertex);
                
                float4 waveValue = tex2Dlod(_WavesTex, float4(o.uv + _Time[1] / 16, 0, 0));
                
                o.vertex.y = o.vertex.y + 0.2 * waveValue.r;
                
                return o;
            }
            
            float4 _ShallowColor;
            float4 _DeepColor;
            float _Distance;
            sampler2D _CameraDepthTexture;
            
            float4 _FoamColor;
            float _FoamDist;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 mainTexColor = tex2D(_MainTex, i.uv);
                float depth = tex2Dproj(_CameraDepthTexture, i.screenPos).r;
                float linearDepth = LinearEyeDepth(depth);
                float depthDiff = (linearDepth - i.screenPos.w) / _Distance;
                depthDiff = saturate(depthDiff);    // Clamp value
                
                if (depthDiff < _FoamDist && depthDiff != 0) {
                    return _FoamColor;
                }
                if (depthDiff == 0) {
                    depthDiff = 1;
                }
                
                fixed4 col = lerp(_ShallowColor, _DeepColor, depthDiff);

                return col * mainTexColor;
            }
            ENDCG
        }
    }
}
