Shader "Unlit/FlagPort"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Progress("_Progress", Range(-1, 1)) = 0
        _Changing("_Changing", Range(0, 1)) = 0
        _Height("_Height", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 
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
                float3 objectVertex : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _Progress;
            float _Changing;
            float _Height;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objectVertex = v.vertex.xyz;
                float stepChange = 1.0 - step(1.0f, abs(_Progress));
                float2 timeShift =  -float2(1.0 - stepChange, stepChange) * _Time.w * 0.1;
                o.uv = TRANSFORM_TEX(v.uv  + timeShift, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed map01(float v){
                return clamp((v + 1.0) * 0.5, 0, 1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Map from +1/-1 to +1/0
                float verticalFalloff = (1.0 - map01(i.objectVertex.y / _Height * 2.0));
                float verticalWave = map01(cos(-i.objectVertex.y+_Time.z));
                float alpha = clamp(0.3 + 0.5 * verticalWave, 0, 1) * verticalFalloff;
                col.a *= alpha;

                //Team Progress (Red to Green)
                float4 white = float4(1.,1.,1., 1.);
                float4 red = float4(1.0, 0.0, 0.0, 1.0);
                float4 green = float4(0.0, 1.0, 0.0, 1.0);
                float4 tintColor = lerp(white, green, clamp(_Progress, 0, 1));
                tintColor *= lerp(white, red, clamp(-_Progress, 0, 1));

                col *= tintColor * lerp(1.0,verticalWave, _Changing);
                col.rgb *= 200.0 * alpha;
                //col.r = verticalFalloff;
                //col.rgba = col.rrrr;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
