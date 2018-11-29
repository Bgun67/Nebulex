// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CameraShaders/Blackout_Shader"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ConciousnessTex("Conciousness",2D) = "white" {}
		_conciousness ("Level of Conciousness", Range(0,10)) = 10
		_BloodTex("Blood",2D) = "white" {}
		_levelOfBlood ("Level of Blood", Range(0,1)) = 1
	}
	SubShader
	{

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"

				sampler2D _MainTex;

				sampler2D _ConciousnessTex;
				fixed _conciousness;

				sampler2D _BloodTex;
				fixed _levelOfBlood;

				struct appdata_t{
					float4 vertex :POSITION;
					float4 color: COLOR;
					float2 texCoord: TEXCOORD0;
				};

				struct v2f{
					float4 vertex :POSITION;
					float4 color: COLOR;
					float2 texCoord: TEXCOORD0;
					float2 screenPos: TEXCOORD1;
				};

				v2f vert(appdata_t IN){
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texCoord = IN.texCoord;
					OUT.color = IN.color;
					OUT.screenPos = ComputeScreenPos(OUT.vertex);

					return OUT;
				}

				fixed4 frag (v2f IN) : COLOR{
					half4 c = tex2D(_MainTex, IN.texCoord) * IN.color;

					half4 conciousness = tex2D(_ConciousnessTex, IN.texCoord) * IN.color;
					float2 worldPos = IN.screenPos * _ScreenParams.xy;
					float4 result = c;
					float sqrScreenX = (IN.screenPos.x-0.5)*(IN.screenPos.x- 0.5);
					float sqrScreenY = (IN.screenPos.y-0.5)*(IN.screenPos.y- 0.5);
					float distFromCenter = float((sqrScreenX+ sqrScreenY)/ _conciousness);
					result.rgb  = c.rgb - float3(1,1,1) * distFromCenter - conciousness * distFromCenter * sin(_Time.y*2.5) * 0.5 + 0.5*(float3(1,1,1) - conciousness) * distFromCenter * sin(_Time.y * 2);//float3(distFromCenter,distFromCenter,distFromCenter);

					//Calculate blood
					half4 blood = tex2D(_BloodTex, IN.texCoord) * IN.color;
					result.rgb = result.rgb - result.rgb * _levelOfBlood * 5/1.5 * distFromCenter * _conciousness* (1-frac(_Time.y * 0.75) * 0.25)  + blood.rgb * _levelOfBlood * 5/2 * distFromCenter * _conciousness * (1-frac(_Time.y * 0.75) * 0.25);

					return result;
				}


				ENDCG
			}
		
	}
}
