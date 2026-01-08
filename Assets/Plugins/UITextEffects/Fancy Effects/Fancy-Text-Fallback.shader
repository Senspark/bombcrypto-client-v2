// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Text Effects/Fancy Text Fallback"
{
	Properties
	{
		_Color ("Main Color", color) = (1, 1, 1, 1)
		_MainTex ("Font Texture", 2D) = "white" {}
		[HideInInspector] _OverlayTex("Overlay Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}
		Lighting Off Cull Off ZTest Always ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
		CGPROGRAM
			#include "UnityCG.cginc"
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ _USEOVERLAYTEXTURE_ON
			#pragma multi_compile __ _USEBEVEL_ON
			#pragma multi_compile __ _USEOUTLINE_ON

			fixed4 _Color;
			sampler2D _MainTex;

			sampler2D _OverlayTex;
			int _OverlayTextureColorMode;

			fixed4 _HighlightColor;
			int _HighlightColorMode;
			fixed4 _ShadowColor;
			int _ShadowColorMode;
			half2 _HighlightOffset;

			fixed4 _OutlineColor;
			half _OutlineThickness;
			int _OutlineColorMode;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;

				float4 tangent : TANGENT;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoordOverlay : TEXCOORD1;

				float4 tangent : TEXCOORD2;
			};
			
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4 _OverlayTex_ST;

			v2f vert(appdata_t v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);

				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.texcoordOverlay = v.texcoord1;

				o.tangent = v.tangent;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float factor = min(i.texcoordOverlay.x, 1);

				fixed4 col = i.color;
				col.a *= tex2D(_MainTex, i.texcoord).a;

				#ifdef _USEOVERLAYTEXTURE_ON
				fixed3 colOverlay = tex2D(_OverlayTex, i.texcoordOverlay - float2(1, 1)).rgb;

				if (_OverlayTextureColorMode == 0)
				{
					col.rgb = lerp(col.rgb, colOverlay, factor);
				}
				else if (_OverlayTextureColorMode == 1)
				{
					col.rgb = col.rgb + colOverlay * factor;
				}
				else
				{
					col.rgb = lerp(col.rgb, col.rgb * colOverlay, factor);
				}
				#endif

				#ifdef _USEBEVEL_ON
				half2 highlightOffset = _HighlightOffset.x * half2(i.tangent.xy) - _HighlightOffset.y * half2(i.tangent.zw);

				col.rgb = lerp(col.rgb, _ShadowColor.rgb, (1 - tex2D(_MainTex, i.texcoord + highlightOffset).a) * _ShadowColor.a * factor);

				col.rgb = lerp(col.rgb, _HighlightColor.rgb, (1 - tex2D(_MainTex, i.texcoord - highlightOffset).a) * _HighlightColor.a * factor);
				#endif

				#ifdef _USEOUTLINE_ON
				float alpha = 8;

				alpha -= tex2D(_MainTex, i.texcoord + float2(0, _OutlineThickness)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(0, -_OutlineThickness)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(_OutlineThickness, 0)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(-_OutlineThickness, 0)).a;

				alpha -= tex2D(_MainTex, i.texcoord + float2(0.707 * _OutlineThickness, 0.707 * _OutlineThickness)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(-0.707 * _OutlineThickness, 0.707 * _OutlineThickness)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(0.707 * _OutlineThickness, -0.707 * _OutlineThickness)).a;
				alpha -= tex2D(_MainTex, i.texcoord + float2(-0.707 * _OutlineThickness, -0.707 * _OutlineThickness)).a;

				alpha = clamp(alpha, 0, 1) * _OutlineColor.a * factor;

				if (_OutlineColorMode == 0)
				{
					col.rgb = lerp(col.rgb, _OutlineColor.rgb, alpha);
				}
				else if (_OutlineColorMode == 1)
				{
					col.rgb = col.rgb + _OutlineColor.rgb * alpha;
				}
				else
				{
					col.rgb = lerp(col.rgb, col.rgb * _OutlineColor.rgb, alpha);
				}
				#endif
				
				return col;
			}
			
		ENDCG
		}
	}
	
	Fallback "Unlit/Texture"
}
