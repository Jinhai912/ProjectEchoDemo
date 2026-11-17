Shader "ECHO/PaperGrid"
{
    Properties
    {
        _PaperColor ("Paper Color", Color) = (0.96, 0.95, 0.91, 1)
        _LineColor ("Line Color", Color) = (0.08, 0.08, 0.08, 1)
        _CellSize ("Cell Size", Float) = 1.0
        _MajorEvery ("Major Every", Float) = 5.0
        _LineWidth ("Line Width", Range(0.001, 0.12)) = 0.008
        _MajorWidth ("Major Line Width", Range(0.001, 0.2)) = 0.014
        _CheckerStrength ("Checker Strength", Range(0, 0.2)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "PaperGrid"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _PaperColor;
                half4 _LineColor;
                float _CellSize;
                float _MajorEvery;
                float _LineWidth;
                float _MajorWidth;
                float _CheckerStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                return output;
            }

            float GridLine(float coordinate, float width)
            {
                float distanceToLine = abs(frac(coordinate + 0.5) - 0.5);
                float antialias = max(fwidth(coordinate) * 1.25, 0.0001);
                return 1.0 - smoothstep(width - antialias, width + antialias, distanceToLine);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldXZ = input.positionWS.xz;
                float2 cellCoordinate = worldXZ / max(_CellSize, 0.0001);
                float2 majorCoordinate = cellCoordinate / max(_MajorEvery, 1.0);

                float minorLine = max(GridLine(cellCoordinate.x, _LineWidth),
                                      GridLine(cellCoordinate.y, _LineWidth));
                float majorLine = max(GridLine(majorCoordinate.x, _MajorWidth),
                                      GridLine(majorCoordinate.y, _MajorWidth));

                float2 cellIndex = floor(cellCoordinate);
                float checker = frac((cellIndex.x + cellIndex.y) * 0.5) > 0.25 ? 1.0 : 0.0;
                half3 paper = _PaperColor.rgb * (1.0 - _CheckerStrength * checker);

                float lineMask = max(minorLine * 0.55, majorLine);
                half3 color = lerp(paper, _LineColor.rgb, lineMask);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
