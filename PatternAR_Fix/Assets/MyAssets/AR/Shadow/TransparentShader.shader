//This is based on a shader from https://alastaira.wordpress.com/2014/12/30/adding-shadows-to-a-unity-vertexfragment-shader-in-7-easy-steps/
// 参考にしたサイト：https://edom18.hateblo.jp/entry/2019/08/04/110439

Shader "Custom/TransparentShadowCollector"
{
	SubShader
	{
		Pass
		{
			// 1.) This will be the base forward rendering pass in which ambient, vertex, and
			// main directional light will be applied. Additional lights will need additional passes
			// using the "ForwardAdd" lightmode.
			// see: http://docs.unity3d.com/Manual/SL-PassTags.html
			//
			// 1.) forwardレンダリングパスのアンビエント、頂点とメインディレクショナルライトで利用されます。
			//     追加ライトの場合は、addtionalパス、ForwardAddライトモードが必要です。
			Tags
			{
				"LightMode" = "ForwardBase" "RenderType" = "Opaque" "Queue" = "Geometry+1" "ForceNoShadowCasting" = "True"
			}

			LOD 150
			Blend Zero SrcColor
			ZWrite On

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

		// 2.) This matches the "forward base" of the LightMode tag to ensure the shader compiles
		// properly for the forward bass pass. As with the LightMode tag, for any additional lights
		// this would be changed from _fwdbase to _fwdadd.
		//
		// 2.) "forward base"ライトモードにマッチし、シェーダがforward baseパスで正常にコンパイルされるようにします。
		//     他の追加ライトの場合は_fwdbaseを_fwdaddに変更する必要があります。
		#pragma multi_compile_fwdbase

		// 3.) Reference the Unity library that includes all the lighting shadow macros
		// 3.) すべてのライティングシャドウマクロのUnityライブラリをインクルードします。
		#include "AutoLight.cginc"


		struct v2f
		{
			float4 pos : SV_POSITION;

			// 4.) The LIGHTING_COORDS macro (defined in AutoLight.cginc) defines the parameters needed to sample 
			// the shadow map. The (0,1) specifies which unused TEXCOORD semantics to hold the sampled values - 
			// As I'm not using any texcoords in this shader, I can use TEXCOORD0 and TEXCOORD1 for the shadow 
			// sampling. If I was already using TEXCOORD for UV coordinates, say, I could specify
			// LIGHTING_COORDS(1,2) instead to use TEXCOORD1 and TEXCOORD2.
			//
			// 4.) LIGHTING_COORDSマクロ（AutoLight.cgincで定義）は、シャドウマップをサンプルするパラメータを定義しています。
			//     (0, 1)はサンプルした値を保持するために、未使用のTEXCOORDを指定します。
			//     このシェーダでは、texcoordsは使用していないため、TEXCOORD0とTEXCOORD1をシャドウのサンプリングのために使用できます。
			//     もしUV座標のためにTEXCOORDを利用していたら、LIGHTHING_COORDS(1, 2)をTEXCOORD1とTEXCOORD2の代わりに使用します。
			LIGHTING_COORDS(0,1)
		};


		v2f vert(appdata_base v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);

			// 5.) The TRANSFER_VERTEX_TO_FRAGMENT macro populates the chosen LIGHTING_COORDS in the v2f structure
			// with appropriate values to sample from the shadow/lighting map
			//
			// 5.) TRANSFER_VERTEX_TO_FRAGMENTマクロは、LIGHTING_COORDSで選択したものをv2f構造体の中で、shadow/lightingマップのサンプル用に適切な値に設定します。
			TRANSFER_VERTEX_TO_FRAGMENT(o);

			return o;
		}

		fixed4 frag(v2f i) : COLOR {

			// 6.) The LIGHT_ATTENUATION samples the shadowmap (using the coordinates calculated by TRANSFER_VERTEX_TO_FRAGMENT
			// and stored in the structure defined by LIGHTING_COORDS), and returns the value as a float.
			//
			// 6.) LIGHT_ATTENUATIONは、シャドウマップからサンプルします。（TRANSFER_VERTEX_TO_FRAGMENTによって計算された座標を使って、LIGHTHING_COORDSによって定義された構造体に保持します）
			//     そしてfloatの値を返します。
			float attenuation = LIGHT_ATTENUATION(i);
			return fixed4(1.0,1.0,1.0,1.0) * attenuation;
		}

		ENDCG
	}
	}

		// 7.) To receive or cast a shadow, shaders must implement the appropriate "Shadow Collector" or "Shadow Caster" pass.
		// Although we haven't explicitly done so in this shader, if these passes are missing they will be read from a fallback
		// shader instead, so specify one here to import the collector/caster passes used in that fallback.
		//
		// 7.) レシーブまたはキャストシャドウのために、シェーダは"Shadow Collector"か"Shadow Caster"パスを適切に実装しなければなりません。
		//     しかしこのシェーダでは明示的にそれをしていませんが、これらのパスが見つからない場合はフォールバックシェーダが代わりに読み込まれます。
		//     フォールバックで使用されるcollector/casterパスをインポートするには、ここでそれを指定します。
			Fallback "VertexLit"

}