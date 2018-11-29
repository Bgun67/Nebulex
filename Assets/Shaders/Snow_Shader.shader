Shader "Custom/Snow_Shader" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Bump ("Bump", 2D) = "bump" {}
		_Snow ("Snow Level", Range(0,1)) = 0
		_SnowColour("Snow Colour", Color) = (1.0,1.0,1.0,1.0)
		_SnowDirection("Snow Direction", Vector) = (0,1,0)
		_SnowDepth("Snow Depth", Range(0, 0.2)) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
		sampler2D _Bump;
		float _Snow;
		float4 _SnowColour;
		float4 _SnowDirection;
		float _SnowDepth;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Bump;
			float3 worldNormal;
			INTERNAL_DATA
		};


		void vert(inout appdata_full v){
			//Convert the normal to world coordinates
			float4 sn = mul(UNITY_MATRIX_IT_MV, _SnowDirection);

			if(dot(v.normal, sn.xyz) >= lerp(1,-1,(_Snow*2)/3)){
				v.vertex.xyz += (sn.xyz + v.normal) * _SnowDepth * _Snow;
			}
		}

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			half4 c = tex2D (_MainTex, IN.uv_MainTex);

			o.Normal = UnpackNormal(tex2D(_Bump, IN.uv_Bump));

			if(dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz) >= lerp(1,-1,_Snow)){
				o.Albedo = _SnowColour.rgb;
				o.Alpha = 1;
			}
			else{
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}



		}
		ENDCG
	}
	FallBack "Diffuse"
}
