Shader "Custom/Particles/Spherical-Force-Field"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Float) = 4.0
        _Center ("Center", Vector) = (0,0,0,0)

        [HDR] _ColourA("Color A", Color) = (0.0,0.0,0.0,0.0)
        [HDR] _ColourB("Color B", Color) = (1.0,1.0,1.0,1.0)

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Opaque" }
        LOD 100
        Blend One One
        ZWrite Off

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
                fixed4 color : COLOR;
                float4 tc0 : TEXCOORD0;
                float4 tc1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 tc0 : TEXCOORD0;
                float4 tc1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float4 _Center;

            float4 _ColourA;
            float4 _ColourB;

            float4 GetParticleOffset(float3 particleCenter){
                float distanceToParticle = distance(particleCenter,_Center);
                
                float distanceToSurface = _Radius - distanceToParticle;
                float3 directionToParticle = normalize(particleCenter - _Center);

                float4 particleOffset;
                particleOffset.xyz = directionToParticle * distanceToSurface;
                particleOffset.w = distanceToSurface/ _Radius;
                return particleOffset* max(0,sign(distanceToSurface));
                
            }

            v2f vert (appdata v)
            {
                v2f o;

                float3 particleCenter = float3(v.tc0.zw,v.tc1.x);

                float3 vertexOffset = GetParticleOffset(particleCenter);

                v.vertex.xyz += vertexOffset;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.color = v.color;
                o.tc0.xy = TRANSFORM_TEX(v.tc0, _MainTex);
                
                o.tc0.zw = v.tc0.zw;
                o.tc1 = v.tc1;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.tc0);

                col *= i.color;

                float3 particleCenter = float3(i.tc0.zw, i.tc1.x);
                float particleOffsetNormalized = GetParticleOffset(particleCenter).w;

                col = lerp(col * _ColourA, col * _ColourB, particleOffsetNormalized);
                col *= col.a;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
