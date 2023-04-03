Shader "SZYU/RenderGroupDisplay" {
	Properties {
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _RGTex("Render Group Texture", 2D) = "white" {}
		[PerRendererData] _RGTex2("Render Group Second Texture", 2D) = "white" {}
		[PerRendererData] _MaskTex("Mask Texture", 2D) = "white" {}
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
		//As the source texes are render textures accumulating
		// premulted colors, we use the merge (1 1-SrcA).
		Blend One OneMinusSrcAlpha
		
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
        
            sampler2D _MainTex;
            sampler2D _RGTex;
            sampler2D _RGTex2;
            sampler2D _MaskTex;
			float _T;

			float4 premult(float4 c) {
				c.rgb *= c.a;
				return c;
			}

			float4 frag(fragment f) : SV_Target {
				float4 c1 = tex2D(_RGTex, f.uv);
				float4 mask = tex2D(_MaskTex, f.uv).a * f.c;
			#ifdef MIX_NONE
				return premult(mask * c1);
			#endif
				float fill = 1;
			#ifdef MIX_FADE
				fill = smoothstep(0, 1, _T);
			#endif
				float4 c2 = tex2D(_RGTex2, f.uv);
				//Transparent textures may have nonzero RGBs
				if (c2.a < 0.001)
					c2 = float4(0,0,0,0);
				float4 c = c1 * (1 - fill) + c2 * fill;
				//return float4(c.r, 0, 0, 1);
				return premult(mask * c);
			}
			ENDCG
		}
	}
}