Shader "Custom/Toon/BumpyLitCutout" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.2
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" }
		Cull Off //draw all faces
		LOD 200
		
		CGPROGRAM
		#pragma surface surf ToonRamp 

		sampler2D _Ramp;

		// custom lighting function that uses a texture ramp based
		// on angle between light direction and normal
		#pragma lighting ToonRamp exclude_path:prepass
		inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
		{
			#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
			#endif
	
			//half d = lerp(0.18, 1, dot (s.Normal, lightDir) * 0.5 + 0.5);
			half d = clamp(dot(s.Normal, lightDir) * 0.5 + 0.5, 0.67, 1);
			half3 ramp = tex2D (_Ramp, float2(d, d)).rgb;
	
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
			c.a = s.Alpha;
			return c;
		}


		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _Color;
		fixed _Cutoff;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float2 uv_BumpMap;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			if (c.a < _Cutoff) {
				discard;
			}
			o.Alpha = c.a * _Color.a;
		}
		ENDCG

	} 

	Fallback "Diffuse"
}
