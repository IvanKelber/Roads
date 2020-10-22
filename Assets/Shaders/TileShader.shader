Shader "Unlit/TileShader"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _OutlineTexture ("Outline Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,0)
        _RenderOutline ("Render Outline", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" 
               "Queue" = "Transparent" 
             }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            sampler2D _MainTex;
            sampler2D _BackgroundTex;
            int _RenderBackground;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                fixed4 background = tex2D(_BackgroundTex, i.uv);
                fixed4 col = background;

                fixed4 main = tex2D(_MainTex, i.uv);
                if(_RenderBackground > 0) {
                    col = lerp(col, main, main.a);
                } else {
                    col = main;
                }        
                
                return col;
            }
            ENDCG
        }
    }
}
