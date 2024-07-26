Shader "Custom/BoidsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _AOMap ("AO Map", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "white" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}
        _BackColor ("Back Color", Color) = (1,1,1,1)
        _BellyColor ("Belly Color", Color) = (1,1,1,1)
        _PatternBlackColor ("Black Color", Color) = (0,0,0,1)
        _PatternWhiteColor ("White Color", Color) = (1,1,1,1)
        _ColorStrength ("Color Strength", Range(0,1)) = 0.5
        _PatternStrength ("Pattern Strength", Range(0,1)) = 1.0
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
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _AOMap;
        sampler2D _HeightMap;
        sampler2D _RoughnessMap;
        fixed4 _BackColor;
        fixed4 _BellyColor;
        float _ColorStrength;
        float _PatternStrength;
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
        float3 _PatternBlackColor;
        float3 _PatternWhiteColor;

        struct Input
        {
            float2 uv_MainTex : TEXCOORD0;
            float4 vertexColor : COLOR;
            float3 localPos : TEXCOORD1;
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

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // アトラスから特定の部分をサンプリング
            float2 uv = frac(IN.uv_MainTex * (1.0 / max(_AtlasSize, 1.0)) + _UvOffset / max(_AtlasSize, 1.0));
            uv = frac(uv * max(_TileAmount, 0.001));
            half4 c = tex2D(_MainTex, uv);

            // パターンの強度を計算（コントラストを上げる）
            float patternIntensity = saturate((length(c.rgb) - 0.5) * 2.0 + 0.5);
            float3 patternColor = lerp(1.0 - _PatternWhiteColor, _PatternWhiteColor, patternIntensity);

            // 背中と腹、側面の色を決定
            float3 objNormal = normalize(IN.vertexColor.gba * 2.0 - 1.0);
            float backFactor = max(objNormal.y, 0.001);
            float bellyFactor = max(-objNormal.y, 0.001);
            float sideFactor = max(1.0 - abs(objNormal.y), 0.001);
            
            // パターンの強さを計算
            float patternFactor = max(_PatternStrength, 0.001);
            patternFactor *= lerp(0.8, 1.2, pow(backFactor, 0.3) + sideFactor * 0.5); // 背中と側面でパターンを強調
            float zFactor = max(0.001, abs(IN.localPos.z + 4.0));
            patternFactor *= saturate((1.0 - zFactor * 0.15) * 2.0); // zFactorの影響をさらに弱める
            patternFactor *= (0.9 + 0.1 * sin(IN.localPos.x * 8.0 + IN.localPos.y * 12.0)); // 微妙な変動を追加
            // アトラス要素絵をぬく
            // 色とパターンを合成
            fixed4 baseColor = lerp(_BellyColor, _BackColor, backFactor + sideFactor * 0.5);
            float patternStrength = saturate(patternFactor * pow(patternIntensity, 1.5)); // パターンの強度を強調
            fixed3 finalColor = lerp(baseColor.rgb, patternColor, patternStrength);
            uv *= 10.;
            // Normal map の適用
            float2 normalUV = RotateUV(uv, _NormalRotation);
            fixed3 normalTex = UnpackNormal(tex2D(_NormalMap, normalUV)).rgb;
            normalTex = lerp(fixed3(0, 0, 1), normalTex, _NormalStrength);

            // AO map の適用
            float2 aoUV = RotateUV(uv, _AORotation);
            float ao = tex2D(_AOMap, aoUV).r;
            ao = lerp(1.0, ao, _AOStrength);

            // Roughness map の適用
            float2 roughnessUV = RotateUV(uv, _RoughnessRotation);
            float roughness = tex2D(_RoughnessMap, roughnessUV).r;
            roughness = lerp(_Glossiness, roughness, _RoughnessStrength);

            // ColorStrengthの影響を調整
            o.Albedo = lerp(baseColor.rgb, finalColor, _ColorStrength * 1.5);
            o.Albedo.rgb += c.rgb * ao;
            o.Normal = normalTex;
            o.Metallic = _Metallic;
            o.Smoothness = roughness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
