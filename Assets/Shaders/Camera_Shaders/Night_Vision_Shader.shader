// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "CameraShaders/Night_Vision_Shader"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_amplification ("Amplification", Range(0,10)) = 2
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

				fixed _amplification;

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
					OUT.screenPos = float2(0,0);

					return OUT;
				}

				fixed4 frag (v2f IN) : COLOR{
					half4 c = tex2D(_MainTex, IN.texCoord) * IN.color;

					float4 result = c;

					//Get the intensity of the white light
					float intensity = (c.r + c.g + c.b)/3;

					//Change the rgb values to green plus a little white light
					result.rgb  = float3(intensity * _amplification, intensity * _amplification * 2,intensity * _amplification);
					return result;
				}


				ENDCG
			}
		
	}
}