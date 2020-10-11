// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Cloak"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _reflectionProbe ("Cloak Probe", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 viewVector : TEXCOORD1;
            };

            sampler2D _MainTex;
            samplerCUBE _reflectionProbe;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewVector = -normalize(UnityWorldSpaceViewDir(worldPos));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //half4 skyData =  UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.viewVector);
                //half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);
                half3 skyColor = texCUBE(_reflectionProbe, i.viewVector);
                // apply fog
                col.rgb = skyColor.rgb * 1.1;
                col.a = 0;
                return col;
            }
            ENDCG
        }
    }
}
