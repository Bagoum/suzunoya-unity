Shader "SZYU/RenderGroupDisplay" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _RGTex("Render Group Texture", 2D) = "white" {}
		[PerRendererData] _RGTex2("Render Group Second Texture", 2D) = "white" {}
		_T("Transition Ratio", Float) = 1
	}
	
	SubShader {
		Tags {
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha, OneMinusDstAlpha One
		
		Pass { 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local __ MIX_NONE
			#pragma multi_compile_local __ MIX_FADE
            #include "UnityCG.cginc"
        
            struct vertex {
                float4 loc  : POSITION;
                float2 uv	: TEXCOORD0;
				float4 color: COLOR;
            };
        
            struct fragment {
                float4 loc  : SV_POSITION;
                float2 uv	: TEXCOORD0;
				float4 c    : COLOR;
            };
        
            fragment vert(vertex v) {
                fragment f;
                f.loc = UnityObjectToClipPos(v.loc);
                f.uv = v.uv;
            	f.c = v.color;
                return f;
            }
        
            sampler2D _RGTex;
            sampler2D _RGTex2;
			float _T;

			float4 frag(fragment f) : SV_Target {
			#ifdef MIX_NONE
				return tex2D(_RGTex, f.uv);
			#endif
				float fill = 1;
			#ifdef MIX_FADE
				fill = smoothstep(0, 1, _T);
			#endif
				return tex2D(_RGTex, f.uv) * (1 - fill) + tex2D(_RGTex2, f.uv) * fill;
			}
			ENDCG
		}
	}
}