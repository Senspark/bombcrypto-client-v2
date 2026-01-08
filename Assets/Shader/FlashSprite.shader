Shader "Custom/FlashSprite" {
    Properties {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass {
        CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct AppData {
                float4 vertex   : POSITION;
                fixed4 color    : COLOR;
                float2 texCoord : TEXCOORD0;
            };

            struct V2F {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texCoord : TEXCOORD0;
            };

            V2F Vert(AppData input) {
                V2F output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texCoord = input.texCoord;
                output.color = input.color;
                return output;
            }

            sampler2D _MainTex;

            fixed4 Frag(V2F input): SV_Target {
                fixed4 color = tex2D(_MainTex, input.texCoord);// * input.color;
                color.rgb = lerp(color.rgb, fixed3(1, 0, 0.9), input.color.a);
                color.rgb *= color.a;
                return color;
            }
        ENDCG
        }
    }
}