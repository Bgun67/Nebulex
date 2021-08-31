// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Cloak"
{
    Properties
    {
        _reflectionProbe ("Cloak Probe", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        
            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf NoLights noshadow noambient nodynlightmap nodirlightmap 

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            half4 LightingNoLights (SurfaceOutput s, half3 lightDir, half atten) {
              half4 c;
              c.rgb = s.Albedo;
              c.a = s.Alpha;
              return c;
          }


            struct Input
            {
                float3 viewDir;
                float3 worldNormal;
                float3 worldPos;
            };

            samplerCUBE _reflectionProbe;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            void surf (Input IN, inout SurfaceOutput o)
            {
                // Albedo comes from a texture tinted by color
                // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
                half3 skyColor = texCUBE(_reflectionProbe, -IN.viewDir);
                o.Albedo = skyColor.rgb;
                //o.Metallic = 0;
                //o.Smoothness = 0;
                //o.Occlusion = 0;
            
            
            }
            ENDCG

            // CGPROGRAM
            // #pragma vertex vert
            // #pragma fragment frag
            // // make fog work
            // #pragma multi_compile_fog

            // #include "UnityCG.cginc"

            // struct appdata
            // {
            //     float4 vertex : POSITION;
            //     float2 uv : TEXCOORD0;
            // };

            // struct v2f
            // {
            //     float2 uv : TEXCOORD0;
            //     UNITY_FOG_COORDS(1)
            //     float4 vertex : SV_POSITION;
            //     half3 viewVector : TEXCOORD1;
            // };

            // sampler2D _MainTex;
            // samplerCUBE _reflectionProbe;
            // float4 _MainTex_ST;

            // v2f vert (appdata v)
            // {
            //     v2f o;
            //     o.vertex = UnityObjectToClipPos(v.vertex);
            //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //     UNITY_TRANSFER_FOG(o,o.vertex);

            //     float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            //     o.viewVector = -normalize(UnityWorldSpaceViewDir(worldPos));
            //     return o;
            // }

            // fixed4 frag (v2f i) : SV_Target
            // {
            //     // sample the texture
            //     fixed4 col = tex2D(_MainTex, i.uv);
            //     //half4 skyData =  UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.viewVector);
            //     //half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
            //     half3 skyColor = texCUBE(_reflectionProbe, i.viewVector);
            //     // apply fog
            //     col.rgb = skyColor.rgb * 1.1;
            //     col.a = 0;
            //     return col;
            // }
            // ENDCG
        
    }
}
