Shader "Unlit/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        dirToSun ("Direction To Sun", Vector) = (0, 0, 0, 0)
        planetCenter ("Planet Center", Vector) = (0, 0, 0, 0)
        atmosphereRadius ("Atmosphere Radius", float) = 0
        oceanRadius ("Ocean Radius", float) = 0
        planetRadius ("Planet Radius", float) = 0

        // float3 planetCenter;

        //     float atmosphereRadius;
        //     float oceanRadius;
        //     float planetRadius;
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        //Tags { "RenderType"="Transparent" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Includes/Math.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _MainTex_ST;

            float3 dirToSun;
            float3 planetCenter;

            float atmosphereRadius;
            float oceanRadius;
            float planetRadius;

            v2f vert (appdata v)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
				// (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                return output;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 originalCol = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hitInfo = raySphere(planetCenter, atmosphereRadius, rayOrigin, rayDir);
                float dstToAtmosphere = hitInfo.x;
                float dstThroughAtmosphere = min(hitInfo.y, sceneDepth - dstToAtmosphere);

                return dstThroughAtmosphere / (atmosphereRadius * 2);
            }
            ENDCG
        }
    }
}
