Shader "Custom/Laser_Bolt"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1
        //_MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front 
        LOD 100

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
        };

        //half _Glossiness;
        //half _Metallic;
        fixed4 _Color;
        float _EmissionStrength;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
           // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
           float albedoStrength = dot(-IN.worldNormal, IN.viewDir);
           float randomnessX = sin(30.0f*(-0.25f*_Time.y + IN.worldPos.x + IN.worldPos.y));
           float randomnessY = sin(20.0f*(0.5f*_Time.y + IN.worldPos.y+ IN.worldPos.z));
           float randomnessZ = sin(25.0f*(0.5f*_Time.y + IN.worldPos.z + IN.worldPos.x));
            o.Emission = (randomnessX*randomnessY*randomnessZ) + albedoStrength *_Color*_EmissionStrength;
            //o.Emission = randomness.xxx;// *(albedoStrength)*randomness * _Color *_EmissionStrength;
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            o.Alpha = albedoStrength + (randomnessX*randomnessY*randomnessZ) * 0.5f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
