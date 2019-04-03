

Shader "Unlit/Shield" {
Properties {
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _Progress("Progress", Range(0,1000)) = 0
    _UVSpeed("UVSpeed", Range(-5,5)) = 0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
           // #pragma multi_compile_particles
            //#pragma multi_compile_fog
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;
            
            struct appdata_t {
                float4 vertex : POSITION;
                uint id : SV_VertexID;
                fixed4 color : COLOR;
                float4 texcoords : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                uint id: VertexID;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                fixed blend : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                
                
            };
            
            float4 _MainTex_ST;
            uint _Progress;
            float _UVSpeed;

            v2f vert (appdata_t v)
            {
                v2f o;
                //UNITY_SETUP_INSTANCE_ID(v);
                //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                
                o.color = v.color * _TintColor;
                
                o.texcoord = TRANSFORM_TEX(v.texcoords.xy + float2(_Time.y * _UVSpeed,0),_MainTex);
               
                o.id = v.id;
                return o;
            }


            
            fixed4 frag (v2f i) : SV_Target
            {
                
                
                fixed4 colA = tex2D(_MainTex, i.texcoord);
                colA.a = 0;
                fixed4 col = 2.0f * (i.color + 0.4 * colA);
               
                //UNITY_APPLY_FOG(i.fogCoord, col);
                clip((float)_Progress - (float)i.id - 1);
                //clip((float)_Progress - (float)i.vertex.x);

                return col;
            }
            ENDCG 
        }
    }   
}
}