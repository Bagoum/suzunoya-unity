Shader "SZYU/RenderGroupDisplay" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _RGTex("Render Group Texture", 2D) = "white" {}
		[PerRendererData] _RGTex2("Render Group Second Texture", 2D) = "white" {}
		[PerRendererData] _MaskTex("Mask Texture", 2D) = "white" {}
		[PerRendererData] _T("Time", Range(0, 10)) = 1
		[PerRendererData] _MaxT("Max Transition Time", Range(0, 10)) = 1
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
		//As the source texes are render textures accumulating
		// premulted colors, we use the merge (1 1-SrcA).
		Blend One OneMinusSrcAlpha
		
		Pass { 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_local __ MIX_FROM_ONLY MIX_TO_ONLY MIX_WIPE_TEX MIX_WIPE1 MIX_WIPE_CENTER MIX_WIPE_Y MIX_ALPHA_BLEND
            #include "UnityCG.cginc"
            #include "Assets/Danmokou/CG/TexMerge.cginc"
        
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
        
            sampler2D _MainTex;
            sampler2D _RGTex;
            sampler2D _RGTex2;
            sampler2D _MaskTex;

			float4 frag(fragment f) : SV_Target {
				float4 mask = tex2D(_MaskTex, f.uv).a * f.c;
				mask.rgb *= mask.a;
				float4 c = MERGE(_RGTex, _RGTex2, f.uv);
				return mask * c;
			}
			ENDCG
		}
	}
}