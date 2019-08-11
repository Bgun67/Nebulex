Shader "Custom/Overdraw_Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Colour ("Tint Color", Color) = (1,0.0,0.0,1.0)
	}
	SubShader
	{
		Fog {Mode Off}
		ZWrite Off
		ZTest Always
		Blend One one //Additive Blending

		Pass{
			SetTexture[_MainTex]{
				constantColor (0.1,0.04,0.02,0)
				combine constant, texture
			}
		}
	}
}
