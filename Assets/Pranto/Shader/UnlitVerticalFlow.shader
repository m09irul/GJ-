Shader "Custom/URP/UnlitVerticalFlowWithEmission"
{
    Properties
    {
        _BaseMap ("Texture (RGB + Alpha)", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint Color", Color) = (1,1,1,1)
        
        [HDR] _EmissionColor ("Emission Color (HDR)", Color) = (0,0,0,1)
        _EmissionStrength ("Emission Strength", Float) = 1.0
        
        _Speed ("Flow Speed", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EmissionColor;
                float _EmissionStrength;
                float _Speed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = pos.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                uv.y += _Time.y * _Speed;

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                // Base tinted color (RGB tinted, alpha from texture * tint alpha)
                half3 baseRGB = texColor.rgb * _BaseColor.rgb;
                half finalAlpha = texColor.a * _BaseColor.a;

                // Emission: add bright HDR color (usually use texture alpha/mask to control where it glows)
                half3 emission = _EmissionColor.rgb * _EmissionStrength * texColor.a;  // glow stronger where texture is opaque

                // Combine: base + emission glow
                half3 finalRGB = baseRGB + emission;

                return half4(finalRGB, finalAlpha);
            }

            ENDHLSL
        }
    }
}