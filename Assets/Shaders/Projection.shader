Shader "Custom/Projection Shader" {
	Properties {
		_Color ("Color tint", Color) = (1.0, 1.0, 1.0, 1.0)
		[NoScaleOffset] _MainTex ("Projected Image", 2D) = "white" {}  
		[MaterialToggle] _isEdgy("is Edgy", Float) = 0
	}
	SubShader {
		Pass {
			ZWrite Off // don't change depths
			Offset -1, -1 // avoid depth fighting
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
 
			CGPROGRAM
 
			#pragma vertex vert
			#pragma fragment frag
 
			// User-specified properties
			uniform fixed4 _Color;
			uniform sampler2D _MainTex; 
			uniform float _isEdgy;
 
			// Projector-specific uniforms
			uniform float4x4 _Projector;
 
			struct vertexInput {
				fixed4 vertex : POSITION;
			};
			struct vertexOutput {
				fixed4 pos : SV_POSITION;
				fixed4 posProj : TEXCOORD0;
			};
 
			vertexOutput vert(vertexInput v) {
				vertexOutput o;
 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.posProj = mul(_Projector, v.vertex);
				return o;
			}
 
			float4 frag(vertexOutput i) : COLOR {
				if (i.posProj.w > 0.0) { // in front of projector?
					fixed4 tex = tex2D(_MainTex , i.posProj.xy / i.posProj.w); 
					if(_isEdgy == 1) {
						return fixed4(tex.xyz * _Color.xyz, normalize(tex.a));
					}
					else {
						return fixed4(tex.xyz * _Color.xyz, tex.a * _Color.a);
					}
				} else // behind projector
               return fixed4(0.0, 0.0, 0.0, 0.0);
			}
			ENDCG
		}
	} 
	Fallback "Projector/Light"
}