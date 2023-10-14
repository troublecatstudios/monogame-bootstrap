Shader "TroubleCat/Sprites/Lit/Default"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        // _MaskTex applies its ALPHA to the final color values.
        _MaskTex("Mask", 2D) = "white" {}
        // _NormalMap is a 2D bump map used for computing lighting values.
        _NormalMap("Normal Map", 2D) = "bump" {}
        // _Emission intensifies the current color value by it's own RGBA
        // to push the color value into HDR space so that the BLOOM filter
        // reacts to it in post processing
        _Emission("Emission Map", 2D) = "black" {}
        // _PaletteTex is a palette swap texture where the UV maps to color values within
        // the _MapTex
        _PaletteTex("Palette Texture", 2D) = "white" {}
        // _MapTex is a 1:1 copy of _MainTex, but all the color values are encoded X,Y coords
        // within the _PaletteTex swap.
        _MapTex("Palette Map Texture", 2D) = "white" {}

        _PixelsPerUnit("Pixels Per Unit", Float) = 1

        // Deprecated. Emission color is sampled directly from the _Emission texture
        [PerRendererData] [HDR] _EmissionColor("Emission Color", Color) = (1,1,1,1)

        // Controls the baseline intensity for Emission Color values. Higher value = more bloom
        // at the cost of color data.
        _EmissionIntensity("Emission Itensity", Float) = 1

        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0

        // Debug props. Setting these to 1 will render the raw map data instead of the final computed texture data.
        [MaterialToggle] ShowEmission("Show Emission Map", Float) = 0
        [MaterialToggle] ShowMainTex("Show Main Texture", Float) = 0
        [MaterialToggle] ShowPalette("Show Palette Map", Float) = 0
        [MaterialToggle] ShowRimLight("Show Rim Light", Float) = 0
        [PerRendererData] ShowMap("Show Map", Float) = 0

        // Whether or not we should process the palette data for this texture.
        [PerRendererData] PaletteEnabled("Enable Palette", Float) = 0

        // Whether or not we should process the rimlighting data for this texture.
        [PerRendererData] RimLight("Enable Rimlight", Float) = 0

        _BlinkColor("Blink Color", Color) = (1,1,1,1)
        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0

    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    ENDHLSL

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __

            #ifdef UNITY_INSTANCING_ENABLED
                UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                    // SpriteRenderer.Color while Non-Batched/Instanced.
                    UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                    // this could be smaller but that's how bit each entry is regardless of type
                    UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
                UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

                #define _RendererColor  UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
                #define _Flip           UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
            #endif // instancing

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                float4  color       : COLOR;
                float2	uv          : TEXCOORD0;
                float2	lightingUV  : TEXCOORD1;
            };

            // this include must occur before the SHAPE_LIGHT(X) defines below
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_Emission);
            SAMPLER(sampler_Emission);
            TEXTURE2D(_PaletteTex);
            SAMPLER(sampler_PaletteTex);
            TEXTURE2D(_MapTex);
            SAMPLER(sampler_MapTex);
            half4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            half4 _NormalMap_ST;
            float4 _PaletteTex_TexelSize;
            float4 _BlinkColor;

            float PaletteEnabled;
            float RimLight;
            float ShowEmission;
            float ShowMainTex;
            float ShowPalette;
            float ShowRimLight;

            float _PixelsPerUnit;

            float4 _Color;
            float4 _EmissionColor;
            float4 _em_c;
            float _EmissionIntensity;

            SHAPE_LIGHT(0)
            SHAPE_LIGHT(1)
            float4 _ShapeLightTexture1_TexelSize;

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            // this include must wait until the above SHAPE_LIGHT(X) defines are done
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float3 AlignToPixelGrid(float3 vertex)
            {
                float3 worldPos = mul(unity_ObjectToWorld, vertex);
                float fudge = 0.5;

                worldPos.x = floor(floor(worldPos.x * _PixelsPerUnit + fudge) / _PixelsPerUnit);
                worldPos.y = floor(floor(worldPos.y * _PixelsPerUnit + fudge) / _PixelsPerUnit);

                return mul(unity_WorldToObject, worldPos);
            }

            float AverageColor(float3 rgb) {
                return (rgb.r + rgb.b + rgb.b) / 3;
            }

            half4 RimLighting(half4 c, float2 luv, float2 uv) {
                if (RimLight == 0) {
                    return c;
                }

                // do some edge detection
                float2 rim = float2(0, 0);
                float addedAlpha = 0;

                float2 aux = _MainTex_TexelSize.xy;
                float value = 0;
                float4 pxMask = float4(0, 0, 0, 0);
                aux.y = 0;

                value = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + aux).a;
                rim.x -= value;
                pxMask.x = 1 - value;
                addedAlpha += value;

                value = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - aux).a;
                rim.x += value;
                pxMask.y = 1 - value;
                addedAlpha += value;

                aux = _MainTex_TexelSize.xy;
                aux.x = 0;
                value = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + aux).a;
                rim.y -= value;
                pxMask.z = 1 - value;
                addedAlpha += value;

                value = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - aux).a;
                rim.y += value;
                pxMask.w = 1 - value;
                addedAlpha += value;
                if (addedAlpha < 4)
                {
                    half4 shapeLight1 = SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, luv);
                    half4 shapeLight1Modulate = shapeLight1 * _ShapeLightBlendFactors1.x;
                    half4 shapeLight1Additive = shapeLight1 * _ShapeLightBlendFactors1.y;

                    half4 light = shapeLight1;
                    half4 finalColor = (_HDREmulationScale * c + shapeLight1 / 2) + shapeLight1Modulate * shapeLight1Additive;
                    finalColor.a = 1;

                    float4 lightMask = float4(0, 0, 0, 0);

                    // try to determine the light source direction
                    aux = _ShapeLightTexture1_TexelSize.xy;
                    aux.y = 0;
                    lightMask.x = AverageColor(SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, luv + aux).rgb);
                    lightMask.y = AverageColor(SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, luv - aux).rgb);

                    aux = _ShapeLightTexture1_TexelSize.xy;
                    aux.x = 0;
                    lightMask.z = AverageColor(SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, luv + aux).rgb);
                    lightMask.w = AverageColor(SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, luv - aux).rgb);

                    float lightMax = max(lightMask.w, max(lightMask.z, max(lightMask.x, lightMask.y)));
                    if (lightMax > 0 && lightMax == lightMask.x && pxMask.x == 1) {
                        // return float4(1, 0, 0, 1) * lightMax;
                        return finalColor;
                    }

                    if (lightMax > 0 && lightMax == lightMask.y && pxMask.y == 1) {
                        // return float4(0, 1, 0, 1) * lightMax;
                        return finalColor;
                    }

                    if (lightMax > 0 && lightMax == lightMask.z && pxMask.z == 1) {
                        // return float4(0, 0, 1, 1) * lightMax;
                        return finalColor;
                    }

                    if (lightMax > 0 && lightMax == lightMask.w && pxMask.w == 1) {
                        // return float4(1, 1, 0, 1) * lightMax;
                        return finalColor;
                    }
                }
                return c;
            }

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;

                float3 alignedPos = AlignToPixelGrid(v.positionOS);

                UNITY_SETUP_INSTANCE_ID(v);

                o.positionCS = TransformObjectToHClip(alignedPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float4 clipVertex = o.positionCS / o.positionCS.w;
                o.lightingUV = ComputeScreenPos(clipVertex).xy;
                o.color = v.color;
                return o;
            }

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                half4 em = SAMPLE_TEXTURE2D(_Emission, sampler_Emission, i.uv);


                // MapTex is a basic Texture2D you might
                // want to change this to a [PerRendererData]
                // and assign via MaterialPropertyBlock
                float4 mapTex = SAMPLE_TEXTURE2D(_MapTex, sampler_MapTex, i.uv);

                // scale up the RED (x) and GREEN (y) values from the color
                int x = round(mapTex.x * 255) / 8;
                int y = round(mapTex.y * 255) / 8;

                // convert the color values into texture space coords
                float2 uv = float2(x * _PaletteTex_TexelSize.x, y * _PaletteTex_TexelSize.y);

                // PaletteTex is a basic Texture2D you might
                // want to change this to a [PerRendererData]
                // and assign via MaterialPropertyBlock
                float4 palette = SAMPLE_TEXTURE2D(_PaletteTex, sampler_PaletteTex, uv);  // read the palette color value

                float4 c;
                c.rgb = main.rgb;
                c.a = main.a;

                if (PaletteEnabled == 1.0) {
                    c.rgb = palette.rgb;
                    c.a = palette.a;
                }

                c.rgb = lerp(c.rgb, _BlinkColor.rgb * _BlinkColor.a, _BlinkColor.a);
                c.a = _Color.a;
                mask.rgb *= mask.a;

                mask.rgb = float3(0, 0, 0);
                mask.a = 0;
                if (c.a < 1) return c;

                if (ShowEmission == 1)
                {
                    c = em * _EmissionIntensity;
                    c.a = em.a;
                }

                if (ShowMainTex == 1)
                {
                    c = main;
                }

                if (ShowPalette == 1)
                {
                    c = mapTex;
                    c.a = main.a;
                }

                if (RimLight == 1)
                {
                    c = RimLighting(c, i.lightingUV, i.uv);
                }

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(c.rgb, c.a, mask, surfaceData);
                InitializeInputData(i.uv, i.lightingUV, inputData);

                half4 combined = CombinedShapeLightShared(surfaceData, inputData);
                combined += em * _EmissionIntensity;
                combined.a = main.a;
                return combined;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color		: COLOR;
                float2 uv			: TEXCOORD0;
                float4 tangent      : TANGENT;
            };

            struct Varyings
            {
                float4  positionCS		: SV_POSITION;
                float4  color			: COLOR;
                float2	uv				: TEXCOORD0;
                float3  normalWS		: TEXCOORD1;
                float3  tangentWS		: TEXCOORD2;
                float3  bitangentWS		: TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float4 _NormalMap_ST;  // Is this the right way to do this?

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.uv = attributes.uv;
                o.color = attributes.color;
                o.normalWS = TransformObjectToWorldDir(float3(0, 0, -1));
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            float4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color		: COLOR;
                float2 uv			: TEXCOORD0;
            };

            struct Varyings
            {
                float4  positionCS		: SV_POSITION;
                float4  color			: COLOR;
                float2	uv				: TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_Emission);
            SAMPLER(sampler_Emission);
            float4 _MainTex_ST;
            float4 _BlinkColor;
            float _EmissionIntensity;

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.uv = attributes.uv;
                o.color = attributes.color;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 em = SAMPLE_TEXTURE2D(_Emission, sampler_Emission, i.uv);

                mainTex.rgb = lerp(mainTex.rgb, _BlinkColor.rgb * _BlinkColor.a, _BlinkColor.a);
                mainTex.rgb += em.rgb * _EmissionIntensity * em.a;
                mainTex.rgb *= mainTex.a;

                return mainTex;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
