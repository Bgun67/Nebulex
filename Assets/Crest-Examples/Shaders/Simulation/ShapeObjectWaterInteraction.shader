﻿// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

Shader "Ocean/Shape/Object Water Interaction"
{
	Properties
	{
		_FactorParallel("FactorParallel", Range(0., 8.)) = 0.2
		_FactorOrthogonal("FactorOrthogonal", Range(0., 4.)) = 0.2
		_Strength("Strength", Range(0., 400.)) = 0.2
		_Velocity("Velocity", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" }
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				fixed4 col : COLOR;
				float offsetDist : TEXCOORD0;
			};

			float _FactorParallel, _FactorOrthogonal;
			float4 _Velocity;
			uniform float _SimDeltaTime;

			v2f vert(appdata_base v)
			{
				v2f o;

				float3 vertexWorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.normal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.)).xyz);

				float3 vel =_Velocity /= 30.;

				float velMag = max(length(vel), 0.001);
				float3 velN = vel / velMag;
				float angleFactor = dot(velN, o.normal);

				if (angleFactor < 0.)
				{
					// this helps for when velocity exactly perpendicular to some faces
					if (angleFactor < -0.0001)
					{
						vel = -vel;
					}

					angleFactor *= -1.;
				}

				float3 offset = o.normal * _FactorOrthogonal * pow(1. - angleFactor, .2) * velMag;
				offset += vel * _FactorParallel * pow(angleFactor, .5);
				o.offsetDist = length(offset);
				vertexWorldPos += offset;

				o.vertex = mul(UNITY_MATRIX_VP, float4(vertexWorldPos, 1.));

				o.col = (fixed4)1.;

				return o;
			}

			float _Strength;
			float _Weight;

			half4 frag(v2f i) : SV_Target
			{
				half4 col = (half4)0.;
				col.x = _Strength * (length(i.offsetDist)) * abs(i.normal.y) * sqrt(length(_Velocity)) / 10.;

				if (dot(i.normal, _Velocity) < -0.1)
				{
					col.x *= -.5;
				}

				// write to both channels of sim. this has the affect of kinematically moving the water, instead of applying
				// a force to accelerate it.
				float dt2 = _SimDeltaTime * _SimDeltaTime;
				return _Weight * half4(col.x*dt2, col.x*dt2, 0., 0.);
			}
			ENDCG
		}
	}
}
