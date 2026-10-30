
Shader "Lpk/LightModel/GrassSway"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (0.5,0.5,0.5,1)

        [Space]
        _ShadowStep ("ShadowStep", Range(0, 1)) = 0.5
        _ShadowStepSmooth ("ShadowStepSmooth", Range(0, 1)) = 0.04

        [Space]
        _SpecularStep ("SpecularStep", Range(0, 1)) = 0.6
        _SpecularStepSmooth ("SpecularStepSmooth", Range(0, 1)) = 0.05
        [HDR] _SpecularColor ("SpecularColor", Color) = (1,1,1,1)

        [Space]
        _RimStep ("RimStep", Range(0, 1)) = 0.65
        _RimStepSmooth ("RimStepSmooth", Range(0,1)) = 0.4
        _RimColor ("RimColor", Color) = (1,1,1,1)

        [Space]
        _OutlineWidth ("OutlineWidth", Range(0.0, 1.0)) = 0.15
        _OutlineColor ("OutlineColor", Color) = (0.0, 0.0, 0.0, 1)

        _WindStrength ("Wind Strength", Range(0,1)) = 0.05
        _WindSpeed ("Wind Speed", Range(0,10)) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ShadowStep;
                float _ShadowStepSmooth;
                float _SpecularStep;
                float _SpecularStepSmooth;
                float4 _SpecularColor;
                float _RimStepSmooth;
                float _RimStep;
                float4 _RimColor;
                float _WindStrength;
                float _WindSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float4 bitangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                float fogCoord : TEXCOORD6;
                float3 positionWS : TEXCOORD7;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float heightMask = saturate(input.positionOS.y);
                float2 windDir = normalize(float2(1, 0.3));

                float sway = sin(
                    _Time.y * _WindSpeed +
                    input.positionOS.x * 0.5 +
                    input.positionOS.z * 0.5
                ) * _WindStrength * heightMask;

                input.positionOS.xz += windDir * sway;

                VertexPositionInputs vertexInput =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInput =
                    GetVertexNormalInputs(input.normalOS, input.tangentOS);

                float3 viewDirWS =
                    GetCameraPositionWS() - vertexInput.positionWS;

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                output.normalWS = float4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = float4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = float4(normalInput.bitangentWS, viewDirWS.z);
                output.viewDirWS = viewDirWS;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;
                float3 N = normalize(input.normalWS.xyz);
                float3 V = normalize(input.viewDirWS);
                float3 L = normalize(_MainLightPosition.xyz);
                float3 H = normalize(V + L);

                // Required by URP's Forward+ light-loop macros.
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = N;
                inputData.viewDirectionWS = V;
                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);

                float NV = dot(N, V);
                float NH = dot(N, H);
                float NL = dot(N, L) * 0.5 + 0.5;

                float4 baseMap = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    uv
                );

                float specularNH = smoothstep(
                    (1 - _SpecularStep * 0.05) - _SpecularStepSmooth * 0.05,
                    (1 - _SpecularStep * 0.05) + _SpecularStepSmooth * 0.05,
                    NH
                );

                float shadowNL = smoothstep(
                    _ShadowStep - _ShadowStepSmooth,
                    _ShadowStep + _ShadowStepSmooth,
                    NL
                );

                float shadow = MainLightRealtimeShadow(input.shadowCoord);

                float rim = smoothstep(
                    (1 - _RimStep) - _RimStepSmooth * 0.5,
                    (1 - _RimStep) + _RimStepSmooth * 0.5,
                    0.5 - NV
                );

                float3 diffuse =
                    _MainLightColor.rgb * baseMap.rgb *
                    _BaseColor.rgb * shadowNL * shadow;

                float3 specular =
                    _SpecularColor.rgb * shadow * shadowNL * specularNH;

                #if defined(_ADDITIONAL_LIGHTS)

                    #if USE_FORWARD_PLUS
                        UNITY_LOOP
                        for (uint lightIndex = 0;
                             lightIndex < min(
                                 URP_FP_DIRECTIONAL_LIGHTS_COUNT,
                                 MAX_VISIBLE_LIGHTS
                             );
                             lightIndex++)
                        {
                            FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

                            Light light = GetAdditionalLight(
                                lightIndex,
                                input.positionWS
                            );

                            float3 addL = normalize(light.direction);
                            float3 addH = normalize(V + addL);
                            float addNL = dot(N, addL) * 0.5 + 0.5;

                            float addShadowNL = smoothstep(
                                _ShadowStep - _ShadowStepSmooth,
                                _ShadowStep + _ShadowStepSmooth,
                                addNL
                            );

                            float addSpecularNH = smoothstep(
                                (1 - _SpecularStep * 0.05) - _SpecularStepSmooth * 0.05,
                                (1 - _SpecularStep * 0.05) + _SpecularStepSmooth * 0.05,
                                dot(N, addH)
                            );

                            float attenuation =
                                light.distanceAttenuation *
                                light.shadowAttenuation;

                            diffuse += light.color * baseMap.rgb *
                                _BaseColor.rgb * addShadowNL * attenuation;

                            specular += _SpecularColor.rgb *
                                addSpecularNH * addShadowNL * attenuation;
                        }
                    #endif

                    uint lightCount = GetAdditionalLightsCount();

                    LIGHT_LOOP_BEGIN(lightCount)
                        Light light = GetAdditionalLight(
                            lightIndex,
                            input.positionWS
                        );

                        float3 addL = normalize(light.direction);
                        float3 addH = normalize(V + addL);
                        float addNL = dot(N, addL) * 0.5 + 0.5;

                        float addShadowNL = smoothstep(
                            _ShadowStep - _ShadowStepSmooth,
                            _ShadowStep + _ShadowStepSmooth,
                            addNL
                        );

                        float addSpecularNH = smoothstep(
                            (1 - _SpecularStep * 0.05) - _SpecularStepSmooth * 0.05,
                            (1 - _SpecularStep * 0.05) + _SpecularStepSmooth * 0.05,
                            dot(N, addH)
                        );

                        float attenuation =
                            light.distanceAttenuation *
                            light.shadowAttenuation;

                        diffuse += light.color * baseMap.rgb *
                            _BaseColor.rgb * addShadowNL * attenuation;

                        specular += _SpecularColor.rgb *
                            addSpecularNH * addShadowNL * attenuation;
                    LIGHT_LOOP_END

                #endif

                float3 ambient =
                    rim * _RimColor.rgb +
                    SampleSH(N) * _BaseColor.rgb * baseMap.rgb;

                float3 finalColor = diffuse + ambient + specular;
                finalColor = MixFog(finalColor, input.fogCoord);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Cull Front
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct OutlineAttributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct OutlineVaryings
            {
                float4 pos : SV_POSITION;
                float fogCoord : TEXCOORD0;
            };

            float _OutlineWidth;
            float4 _OutlineColor;
            float _WindStrength;
            float _WindSpeed;

            OutlineVaryings vertOutline(OutlineAttributes v)
            {
                OutlineVaryings o;

                float heightMask = saturate(v.vertex.y);
                float2 windDir = normalize(float2(1, 0.3));

                float sway = sin(
                    _Time.y * _WindSpeed +
                    v.vertex.x * 0.5 +
                    v.vertex.z * 0.5
                ) * _WindStrength * heightMask;

                v.vertex.xz += windDir * sway;

                VertexPositionInputs vertexInput =
                    GetVertexPositionInputs(v.vertex.xyz);

                float3 outlinePosition =
                    v.vertex.xyz + v.normal * _OutlineWidth * 0.1;

                o.pos = TransformObjectToHClip(outlinePosition);
                o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

                return o;
            }

            float4 fragOutline(OutlineVaryings i) : SV_Target
            {
                float3 finalColor = MixFog(_OutlineColor.rgb, i.fogCoord);
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
