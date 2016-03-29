Shader "Cg  shader for billboards" {
	Properties{
		_Color("Color tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex("Texture Image", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.2
		}
	SubShader{ 
		Blend SrcAlpha OneMinusSrcAlpha
		Pass{
			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 

			// User-specified uniforms       
			uniform fixed4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed _Cutoff;

			struct vertexInput {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};

			vertexOutput vert(vertexInput input) {
				vertexOutput output;
				float scaleX = length(mul(_Object2World, float4(1.0, 0.0, 0.0, 0.0)));
				float scaleY = length(mul(_Object2World, float4(0.0, 1.0, 0.0, 0.0)));
				output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
				output.pos.x = (mul(UNITY_MATRIX_P,
					mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
					+ float4(input.vertex.x * scaleX, input.vertex.y * scaleY, 0.0, 0.0))).x;

				output.tex = input.tex;

				return output;
			}

			float4 frag(vertexOutput input) : COLOR {
				fixed4 tex = tex2D(_MainTex, input.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				if (tex.a < _Cutoff) {
					discard;
				}
				//return tex2D(_MainTex, float2(input.tex.xy));
				return fixed4(tex.xyz * _Color.xyz, tex.a * _Color.a);
			}

			ENDCG
		}
	}
}