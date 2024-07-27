Shader "Custom/ConditionalColorAnalysis"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ConditionTex ("Condition Texture", 2D) = "white" {}
        _Resolution ("Texture Resolution", Vector) = (256, 256, 0, 0)
        _Threshold ("Threshold", Float) = 0.5
        _isSameColor ("Is SameColor", Float) = 0.0 // 0.0 for less than, 1.0 for greater or equal
        _TargetColor ("Target Color", Color) = (1,1,1,1)
        _NeighborhoodSize("NeighborwoodSize",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex, _ConditionTex;
            float2 _Resolution;
            float _Threshold;
            float _isSameColor;
            float4 _TargetColor;
            float _NeighborhoodSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texelSize = 1.0 / _Resolution;
                int countTarget = 0;
                fixed4 sum = tex2D(_ConditionTex, uv);
                float4 centerCol = tex2D(_MainTex, uv);

                float4 compareColor;

                // _isSameColor に基づいた compareColor の設定
                if (_isSameColor > 0.0) {
                    // 中心色の反対色
                    compareColor.rgb = 1.0 - centerCol.rgb;
                    compareColor.a = 1.0;
                } else if (_isSameColor < 0.0) {
                    // 中心色そのもの
                    compareColor = centerCol;
                } else {
                    // 指定された _TargetColor
                    compareColor = _TargetColor;
                }
                
                // 周囲のピクセルと compareColor を比較
                for (int x = -_NeighborhoodSize; x <= _NeighborhoodSize; x++) {
                    for (int y = -_NeighborhoodSize; y <= _NeighborhoodSize; y++) {
                        if (x == 0 && y == 0) continue;  // 中心ピクセルはスキップ
                        float4 neighborColor = tex2D(_MainTex, uv + float2(x, y) * texelSize);
                        if (length(neighborColor.rgb - compareColor.rgb) < 0.01) {
                            countTarget++;
                        }
                    }
                }

                // 閾値に基づいた処理
                if (countTarget >= _Threshold) {
                    sum.rgb = 1.0;  // 色を反転
                }else{
                    sum.rgb = 0.0;
                }

                //sum = sum + abs(sin(_Time.y * 100. + uv.x*90 ));
                //sum *= step(sin(uv.x * 10.+_Time.y),0. );

                return sum;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
