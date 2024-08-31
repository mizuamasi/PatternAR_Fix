Shader "Custom/BoidsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _AOMap ("AO Map", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _BackColor ("Pattern Color", Color) = (1,1,1,1)
        _BellyColor ("Base Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0,1)) = 0.5
        _PatternStrength ("Pattern Strength", Range(0,1)) = 1.0
        _BaseColorStrength ("Base Color Strength", Range(0,1)) = 0.5
        _LightInfluence ("Light Influence", Range(0,1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _NormalRotation ("Normal Map Rotation", Range(0,360)) = 0.0
        _AORotation ("AO Map Rotation", Range(0,360)) = 0.0
        _RoughnessRotation ("Roughness Map Rotation", Range(0,360)) = 0.0
        _NormalStrength ("Normal Map Strength", Range(0,1)) = 1.0
        _AOStrength ("AO Map Strength", Range(0,1)) = 1.0
        _RoughnessStrength ("Roughness Map Strength", Range(0,1)) = 1.0
        _TailPhaseOffset ("Tail Phase Offset", Float) = 0.0
        _TailFrequency ("Tail Frequency", Float) = 2.0
        _TailAmplitude ("Tail Amplitude", Float) = 0.5
        _TailSwingPhase ("Tail Swing Phase", Float) = 0.0
        _TailFrequencyMultiplier ("Tail Frequency Multiplier", Float) = 1.0
        _BodyBendAmount ("Body Bend Amount", Float) = 0.05
        _UvOffset ("UV Offset", Vector) = (0,0,0,0)
        _AtlasSize ("Atlas Size", Float) = 4
        _TileAmount ("Tile Amount", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf StandardCustom fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _AOMap;
        sampler2D _HeightMap;
        sampler2D _RoughnessMap;
        fixed4 _BackColor;
        fixed4 _BellyColor;
        float _ColorStrength;
        float _PatternStrength;
        float _BaseColorStrength;
        float _LightInfluence;
        half _Glossiness;
        half _Metallic;
        float _TailFrequencyMultiplier;
        float _TailPhaseOffset;
        float _TailFrequency;
        float _TailAmplitude;
        float _BodyBendAmount;
        float _TailSwingPhase;
        float2 _UvOffset;
        float _AtlasSize;
        float _TileAmount;
        float _NormalRotation;
        float _AORotation;
        float _RoughnessRotation;
        float _NormalStrength;
        float _AOStrength;
        float _RoughnessStrength;

        struct Input
        {
            float2 uv_MainTex : TEXCOORD0;
            float4 vertexColor : COLOR;
            float3 localPos : TEXCOORD1;
        };

        struct SurfaceOutputStandardCustom
        {
            fixed3 Albedo;
            fixed3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            fixed Alpha;
        };

        float2 RotateUV(float2 uv, float rotation)
        {
            float rad = rotation * 0.01745329252; // degrees to radians
            float2x2 rotationMatrix = float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
            return mul(rotationMatrix, uv - 0.5) + 0.5;
        }

        void vert(inout appdata_full v)
        {
            float tailEffect = saturate(-v.vertex.z / 3.0); // 尻尾に近いほど効果大
            float tailPhase = _TailSwingPhase + v.vertex.z / 3.0 * _TailFrequency * _TailFrequencyMultiplier + _TailPhaseOffset;
            float bodyBend = sin(tailPhase * _BodyBendAmount * (1.0 - v.vertex.z)); // 頭に近いほど曲げを小さく
            
            v.vertex.x += sin(tailPhase) * tailEffect * _TailAmplitude + bodyBend;
        }

        // カスタムライティング関数
        half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi)
        {
            // 標準のPBRライティングを計算
            SurfaceOutputStandard r;
            r.Albedo = s.Albedo;
            r.Normal = s.Normal;
            r.Emission = s.Emission;
            r.Metallic = s.Metallic;
            r.Smoothness = s.Smoothness;
            r.Occlusion = s.Occlusion;
            r.Alpha = s.Alpha;
            
            half4 pbr = LightingStandard(r, viewDir, gi);
            
            // ライトの影響を調整
            pbr.rgb = lerp(s.Albedo, pbr.rgb, _LightInfluence);
            
            return pbr;
        }

        void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi)
        {
            UNITY_GI(gi, s, data);
        }

        void surf (Input IN, inout SurfaceOutputStandardCustom o)
        {
            // アトラスから特定の部分をサンプリング
            float2 uv = frac(IN.uv_MainTex * (1.0 / max(_AtlasSize, 1.0)) + _UvOffset / max(_AtlasSize, 1.0));
            uv = frac(uv * max(_TileAmount, 0.001));
            uv = IN.uv_MainTex.xy;
            half4 c = tex2D(_MainTex, uv);
            uv /= 20.;
            
            // パターンの強度を計算（コントラストを上げる）
            float patternIntensity = saturate((length(c.rgb) - 0.5) * 2.0 + 0.5);
            
            float3 baseColor = _BellyColor.rgb;
            float3 patternColor = _BackColor.rgb;
            
            float3 objNormal = normalize(IN.vertexColor.gba * 2.0 - 1.0);
            float backFactor = max(objNormal.y, 0.001);
            float bellyFactor = max(-objNormal.y, 0.001);
            float sideFactor = max(1.0 - abs(objNormal.y), 0.001);
            
            // パターンの強さを計算
            float patternFactor = max(_PatternStrength, 0.001);
            patternFactor *= lerp(0.8, 1.2, pow(backFactor, 0.3) + sideFactor * 0.5);
            float zFactor = max(0.001, abs(IN.localPos.z + 4.0));
            patternFactor *= saturate((1.0 - zFactor * 0.15) * 2.0);
            patternFactor *= (0.9 + 0.1 * sin(IN.localPos.x * 8.0 + IN.localPos.y * 12.0));
            
            // 色とパターンを合成
            float patternStrength = saturate(patternFactor * pow(patternIntensity, 1.5));
            fixed3 finalColor = lerp(baseColor, patternColor, patternStrength * _PatternStrength);
            
            // ベースカラーの強調
            finalColor = lerp(finalColor, baseColor, _BaseColorStrength);
            
            uv *= 33.;
            
            // Normal map の適用
            float2 normalUV = RotateUV(uv, _NormalRotation);
            fixed3 normalTex = UnpackNormal(tex2D(_NormalMap, normalUV)).rgb;
            normalTex = lerp(fixed3(0, 0, 1), normalTex, _NormalStrength);

            // AO map の適用
            float2 aoUV = RotateUV(uv, _AORotation);
            float ao = tex2D(_AOMap, aoUV).r;

            // Roughness map の適用
            float2 roughnessUV = RotateUV(uv, _RoughnessRotation);
            float roughness = tex2D(_RoughnessMap, roughnessUV).r;
            roughness = lerp(_Glossiness, roughness, _RoughnessStrength);

            // Albedoの設定
            o.Albedo = finalColor;
            o.Albedo.rgb += c.rgb * ao; // AOの影響を弱める
            o.Normal = normalTex;
            o.Metallic = _Metallic;
            o.Smoothness = roughness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}