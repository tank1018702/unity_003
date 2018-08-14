// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Matcap shader
// Copyright (c) Staggart Creations
// contact@staggart.xyz

Shader "Staggart Creations/Matcap"
{
	Properties
	{
		[NoScaleOffset] _Matcap ("Matcap texture", 2D) = "white" { }
		[NoScaleOffset] _MainTex ("Diffuse", 2D) = "white" { }
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" { }
		_OutlineSize ("Size", Range(0.0, 0.02)) = 0.002
		_OutlineColor ("Color", Color) = (0, 0, 0, 1)
	}
	CGINCLUDE
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile_fwdbase
	#pragma shader_feature _NORMALMAP
	
	#include "UnityCG.cginc" // for UnityObjectToWorldNormal
	#include "AutoLight.cginc" // for shadows
	
	//Compatibility for Unity 5.3
		#if UNITY_VERSION < 540

		#define OBJECT2WORLD unity_ObjectToWorld

		#define WORLD2OBJECT unity_WorldToObject

		#else

		#define OBJECT2WORLD unity_ObjectToWorld

		#define WORLD2OBJECT unity_WorldToObject

		#endif

	
	half _OutlineSize;
	half4 _OutlineColor;
	sampler2D _Matcap;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	
	//------ STRUCTS ------//
	
	//Vertex data input
	struct vertIn
	{
		float2 uv: TEXCOORD0;
		float4 vertex: POSITION;
		float3 normal: NORMAL;
		float4 tangent: TANGENT;
		fixed4 color: COLOR;
	};
	
	//Vertex data output
	struct vertOut
	{
		float2 uv: TEXCOORD0;
		float4 pos: POSITION;
		float3 normal: NORMAL;
		half3 ambientColor: COLOR;
		
		#ifdef _NORMALMAP

		float3 normalWorld: TEXCOORD1;
		float3 tangentWorld: TEXCOORD2;
		float3 binormalWorld: TEXCOORD3;
		#endif

		
		LIGHTING_COORDS(4, 5)
		UNITY_FOG_COORDS(6)
	};
	
	ENDCG
	
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+0" }
		
		//#startinsert
		Pass
		{
			Name "OUTLINE"
			
			Cull Front
			
			CGPROGRAM
			
			vertOut vert(vertIn v)
			{
				
				vertOut o;
				UNITY_INITIALIZE_OUTPUT(vertOut, o)
				
				o.pos = UnityObjectToClipPos(v.vertex);
				half3 normal = mul((half3x3)UNITY_MATRIX_IT_MV, v.normal);
				half2 offset = TransformViewToProjection(normal.xy);
				o.pos.xy += offset * o.pos.z * (_OutlineSize);
				return o;
			}
			
			fixed4 frag(vertOut i): COLOR
			{
				fixed4 col = _OutlineColor;
				return col;
			}
			ENDCG
		}
		//#endinsert
		
		
		Pass
		{
			Name "MATCAP"
			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM
			
			//------ SHADERS ------//
			
			//Vertex shader
			vertOut vert(vertIn v)
			{
				vertOut o;
				UNITY_INITIALIZE_OUTPUT(vertOut, o);
				
				/*-- Output --*/
				o.uv = v.uv.xy;
				//Position vertices in world
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				
				#ifdef _NORMALMAP

				o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.tangentWorld = normalize(mul(unity_ObjectToWorld, v.tangent).xyz);
				o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld));
				#endif

				
				o.ambientColor = ShadeSH9(half4(v.normal, 1)).rgb;
				
				// Calculates shadow and light attenuation and passes it to the frag shader.
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				//Shadow receiving
				TRANSFER_SHADOW(o);
				//Fog support
				UNITY_TRANSFER_FOG(o, o.pos);
				
				//Pass to fragment shader
				return o;
			}
			
			//Fragment shader
			fixed4 frag(vertOut i): SV_TARGET
			{
				#ifdef _NORMALMAP

				float3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.xy));
				float3x3 local2WorldTranspose = float3x3(
				i.tangentWorld,
				i.binormalWorld,
				i.normalWorld
				);
				normal = normalize(mul(normal, local2WorldTranspose));
				#else

				float3 normal = i.normal;
				#endif

				
				fixed2 remapUV = (mul(UNITY_MATRIX_V, float4(normal, 0)).xyz.rgb.rg * 0.5 + 0.5);
				
				float4 matcapLookup = tex2D(_Matcap, remapUV);
				float4 diffuse = tex2D(_MainTex, i.uv.xy);
				
				fixed4 color = fixed4(diffuse * matcapLookup.rgb * (i.ambientColor.rgb + LIGHT_ATTENUATION(i)), 1);
				
				//Apply fog
				UNITY_APPLY_FOG(i.fogCoord, color);
				
				return color;
			}
			
			ENDCG
		}// Pass
		
		// shadow casting support
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}//Subshader
	
	CustomEditor "ProceduralMatcapGUI"
}
