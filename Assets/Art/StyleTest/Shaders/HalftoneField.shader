Shader "ECHO/HalftoneField"
{
    Properties
    {
        _Color ("Ink Color", Color) = (0.08, 0.08, 0.08, 1)
        _Density ("Dots Per Unit", Range(0.1, 12)) = 2.2
        _DotSize ("Dot Size", Range(0.02, 0.45)) = 0.18
        _ClusterScale ("Cluster Scale", Range(0.05, 2)) = 0.28
        _Seed ("Seed", Float) = 7.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Halftone"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Density;
                float _DotSize;
                float _ClusterScale;
                float _Seed;
            CBUFFER_END

            float Hash(float2 value)
            {
                return frac(sin(dot(value, float2(12.9898, 78.233)) + _Seed) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldXZ = input.positionWS.xz;
                float2 warp = float2(
                    Hash(floor(worldXZ * 0.18) + 11.3),
                    Hash(floor(worldXZ * 0.18) + 27.1)
                ) - 0.5;
                float2 grid = (worldXZ + warp * 1.1) * _Density;
                float2 cell = floor(grid);
                float2 local = frac(grid) - 0.5;

                float dotHash = Hash(cell);
                float radius = _DotSize * (0.35 + 0.65 * dotHash);
                float distanceToCenter = length(local);
                float antialias = max(fwidth(distanceToCenter) * 1.5, 0.001);
                float dotMask = 1.0 - smoothstep(radius - antialias, radius + antialias, distanceToCenter);

                float cluster = Hash(floor(worldXZ * _ClusterScale) + 19.7);
                float clusterMask = 0.12 + 0.88 * smoothstep(0.18, 0.82, cluster);

                float2 centeredUV = abs(input.uv - 0.5) * 2.0;
                float edgeMask = 1.0 - smoothstep(0.72, 1.0, max(centeredUV.x, centeredUV.y));

                return half4(_Color.rgb, dotMask * clusterMask * edgeMask * _Color.a);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
