Shader "Custom/OutlineShader"
{
    Properties
    {
		[Header(_General Properties_)]
        _MainTex ("Main Texture", 2D) = "white" {}	//0
		_Color("Main Color", Color) = (1,1,1,1)		//1
		_Alpha("General Alpha",  Range(0,1)) = 1	//2
		_RenderOutline("Render Outline", Int) = 0

		
		[Header(_Outline Basic Properties_)]
		_OutlineColor("Outline Base Color", Color) = (1,1,1,1) //14
		_OutlineAlpha("Outline Base Alpha",  Range(0,1)) = 1 //15
		_OutlineGlow("Outline Base Glow", Range(1,100)) = 1.5 //16
		[Toggle()] _Outline8Directions("Outline Base High Resolution?", float) = 0 //17
		_OutlineWidth("Outline Base Width", Range(0,0.2)) = 0.004 //18
		[Header(_Outline Width_)]
		[Toggle()] _OutlineIsPixel("Outline Base is Pixel Perfect?", float) = 0 //19
		_OutlinePixelWidth("Outline Base Pixel Width", Int) = 1 //20
		
		[Space]
		[Header(_Outline Texture_)]
		// [Toggle()] _OutlineTexToggle("Outline uses texture?", float) = 0 //21
		_OutlineTex("Outline Texture", 2D) = "white" {} //22
		_OutlineTexXSpeed("Outline Texture scroll speed X axis", Range(-50,50)) = 10 //23
		_OutlineTexYSpeed("Outline Texture scroll speed Y axis", Range(-50,50)) = 0 //24
		[Toggle()] _OutlineTexGrey("Outline Texture is Greyscaled?", float) = 0 //25

		_MySrcMode ("SrcMode", Float) = 5 // 131
		_MyDstMode ("DstMode", Float) = 10 // 132


		[Space]
		[Header(_Outline Distortion_)]
		[Toggle()] _OutlineDistortToggle("Outline uses distortion?", float) = 0 //26
		_OutlineDistortTex("Outline Distortion Texture", 2D) = "white" {} //27
		_OutlineDistortAmount("Outline Distortion Amount", Range(0,2)) = 0.5 //28
		_OutlineDistortTexXSpeed("Outline Distortion scroll speed X axis", Range(-50,50)) = 5 //29
		_OutlineDistortTexYSpeed("Outline Distortion scroll speed Y axis", Range(-50,50)) = 5 //30


		[Header(This effect will place the inner outlines over the original sprite)]
		_InnerOutlineColor("Inner Outline Color", Color) = (1,0,0,1) //66
		_InnerOutlineThickness("Inner Outline Thickness",  Range(0,3)) = 1 //67
		_InnerOutlineAlpha("Inner Outline Alpha",  Range(0,1)) = 1 //68
		_InnerOutlineGlow("Inner Outline Glow",  Range(1,250)) = 1 //69

		_AlphaCutoffValue("Alpha cutoff value", Range(0, 1)) = 0.25 //70

		[Toggle()] _OnlyOutline("Only render outline?", float) = 0 //71
		[Toggle()] _OnlyInnerOutline("Only render innerr outline?", float) = 0 //72

    }

    SubShader
    {
		Tags { "Queue" = "Transparent" 
				"CanUseSpriteAtlas" = "True" 
				"IgnoreProjector" = "True" 
				"RenderType" = "Transparent" 
				"PreviewType" = "Plane" }
		Blend [_MySrcMode] [_MyDstMode]
		

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			
			#pragma shader_feature OUTBASE_ON
			#pragma shader_feature ONLYOUTLINE_ON
			#pragma shader_feature INNEROUTLINE_ON
			#pragma shader_feature ONLYINNEROUTLINE_ON
	
			#pragma shader_feature OUTTEX_ON
			#pragma shader_feature OUTDIST_ON
			#pragma shader_feature OUTBASE8DIR_ON
			#pragma shader_feature OUTBASEPIXELPERF_ON
			#pragma shader_feature OUTGREYTEXTURE_ON
			#pragma shader_feature COLORRAMPOUTLINE_ON
			#pragma shader_feature GREYSCALEOUTLINE_ON
			#pragma shader_feature POSTERIZEOUTLINE_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
				half4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
				half4 color : COLOR;
				// #if OUTTEX_ON
				half2 uvOutTex : TEXCOORD1;
				// #endif
				// #if OUTDIST_ON
				half2 uvOutDistTex : TEXCOORD2;
				// #endif
            };

            sampler2D _MainTex;
            half4 _MainTex_ST, _MainTex_TexelSize, _Color;
			half _Alpha, _RandomSeed;

			
			half4 _OutlineColor;
			half _OutlineAlpha, _OutlineGlow, _OutlineWidth;
			int _OutlinePixelWidth;

			// #if OUTTEX_ON
			sampler2D _OutlineTex;
			half4 _OutlineTex_ST;
			half _OutlineTexXSpeed, _OutlineTexYSpeed;
			// #endif

			// #if OUTDIST_ON
			sampler2D _OutlineDistortTex;
			half4 _OutlineDistortTex_ST;
			half _OutlineDistortTexXSpeed, _OutlineDistortTexYSpeed, _OutlineDistortAmount;
			// #endif

			int _RenderOutline;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				// #if OUTTEX_ON
				o.uvOutTex = TRANSFORM_TEX(v.uv, _OutlineTex);
				// #endif

				// #if OUTDIST_ON
				o.uvOutDistTex = TRANSFORM_TEX(v.uv, _OutlineDistortTex);
				// #endif

                return o;
            }

			half3 GetPixel(in int offsetX, in int offsetY, half2 uv, sampler2D tex)
			{
				return tex2D(tex, (uv + half2(offsetX * _MainTex_TexelSize.x, offsetY * _MainTex_TexelSize.y))).rgb;
			}

            half4 frag (v2f i) : SV_Target
            {
				half2 uvRect = i.uv;

			
				half4 col = tex2D(_MainTex, i.uv) * i.color;
				half originalAlpha = col.a;
				col.rgb *= col.a;


				//OUTLINE-------------------------------------------------------------
				// #ifdef OUTBASEPIXELPERF_ON
				// half2 destUv = half2(_OutlinePixelWidth * _MainTex_TexelSize.x, _OutlinePixelWidth * _MainTex_TexelSize.y);
				// #else
				// half2 destUv = half2(_OutlineWidth * _MainTex_TexelSize.x * 200, _OutlineWidth * _MainTex_TexelSize.y * 200);
				// #endif
				half2 destUv = half2(_OutlineWidth * _MainTex_TexelSize.x * 200, _OutlineWidth * _MainTex_TexelSize.y * 200);

				#if OUTDIST_ON
				i.uvOutDistTex.x += ((_Time + _RandomSeed) * _OutlineDistortTexXSpeed) % 1;
				i.uvOutDistTex.y += ((_Time + _RandomSeed) * _OutlineDistortTexYSpeed) % 1;
				#if ATLAS_ON
				i.uvOutDistTex = half2((i.uvOutDistTex.x - _MinXUV) / (_MaxXUV - _MinXUV), (i.uvOutDistTex.y - _MinYUV) / (_MaxYUV - _MinYUV));
				#endif
				half outDistortAmnt = (tex2D(_OutlineDistortTex, i.uvOutDistTex).r - 0.5) * 0.2 * _OutlineDistortAmount;
				destUv.x += outDistortAmnt;
				destUv.y += outDistortAmnt;
				#endif

				half spriteLeft = tex2D(_MainTex, i.uv + half2(destUv.x, 0)).a;
				half spriteRight = tex2D(_MainTex, i.uv - half2(destUv.x, 0)).a;
				half spriteBottom = tex2D(_MainTex, i.uv + half2(0, destUv.y)).a;
				half spriteTop = tex2D(_MainTex, i.uv - half2(0, destUv.y)).a;
				half result = spriteLeft + spriteRight + spriteBottom + spriteTop;

				// half spriteTopLeft = tex2D(_MainTex, i.uv + half2(destUv.x, destUv.y)).a;
				// half spriteTopRight = tex2D(_MainTex, i.uv + half2(-destUv.x, destUv.y)).a;
				// half spriteBotLeft = tex2D(_MainTex, i.uv + half2(destUv.x, -destUv.y)).a;
				// half spriteBotRight = tex2D(_MainTex, i.uv + half2(-destUv.x, -destUv.y)).a;
				// result = result + spriteTopLeft + spriteTopRight + spriteBotLeft + spriteBotRight;
				
				result = step(0.05, saturate(result));

				// #if OUTTEX_ON
				i.uvOutTex.x += (_Time * _OutlineTexXSpeed) % 1;
				i.uvOutTex.y += (_Time * _OutlineTexYSpeed) % 1;

				half4 tempOutColor = tex2D(_OutlineTex, i.uvOutTex);
				tempOutColor *= _OutlineColor;
				_OutlineColor = tempOutColor;
				// #endif

				result *= (1 - originalAlpha) *_OutlineAlpha;

				half4 outline = result * _OutlineColor;
				outline.rgb *= _OutlineGlow * 2;
	
				//-----------------------------------------------------------------------------

				if(_RenderOutline > 0) {
					col += outline;
				}			
				

				col *= _Color;
                return col;
            }
            ENDCG
        }
    }
}