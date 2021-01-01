// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BackgroundObjects"{
// Properties {
//     _Color ("Main Color", Color) = (1,1,1,1)
//     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
//     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
//     [Enum(Equal,3,NotEqual,6)] _StencilTest ("Stencil Test", int) = 6
// }

// SubShader {
//     Tags {"Queue"="Transparent" "RenderType" ="Transparent"}
//     ZWrite off
//     Blend SrcAlpha OneMinusSrcAlpha
// Stencil{
//                  Ref 1
//                  Comp NotEqual
//                  Pass keep
//              }
//     Pass {
    
// }   

// CGPROGRAM
// #pragma surface surf Lambert alphatest:_Cutoff

// sampler2D _MainTex;
// fixed4 _Color;

// struct Input {
//     float2 uv_MainTex;
// };

// void surf (Input IN, inout SurfaceOutput o) {
//     fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
//     o.Albedo = c.rgb;
//     o.Alpha = c.a;
// }
// ENDCG
// }

// Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
// }
    
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}

SubShader {
    Tags { "Queue"="Geometry-1" }
    Lighting on
	Stencil {	
                Ref 1
                Comp Equal
            }
    CGPROGRAM
	
#pragma surface surf Lambert alphatest:_Cutoff

sampler2D _MainTex;
fixed4 _Color;

struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}
}


 //
     
// Properties {
//     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
//     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
// }

// SubShader {
//     Tags { "Queue"="Geometry" }
    

//     Stencil {
//                 Ref 0
//                 Comp NotEqual
//                 Pass Replace
//             }

//     Pass {
        
//         CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #pragma target 2.0
//             #pragma multi_compile_fog

 
//             #include "UnityCG.cginc"
 
//             struct appdata_t {
//                 float4 vertex : POSITION;
//                 float2 texcoord : TEXCOORD0;
//                 UNITY_VERTEX_INPUT_INSTANCE_ID
//             };
 
//             struct v2f {
//                 float4 vertex : SV_POSITION;
//                 float2 texcoord : TEXCOORD0;
//                 UNITY_FOG_COORDS(1)
//                 UNITY_VERTEX_OUTPUT_STEREO
//             };
 
//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             fixed _Cutoff;
 
//             v2f vert (appdata_t v)
//             {
//                 v2f o;
//                 UNITY_SETUP_INSTANCE_ID(v);
//                 UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
//                 UNITY_TRANSFER_FOG(o,o.vertex);
//                 return o;
//             }
 
//             float4 _LightColor0;

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 fixed4 col = tex2D(_MainTex, i.texcoord);
//                 clip(col.a - _Cutoff);
//                 UNITY_APPLY_FOG(i.fogCoord, col);
//                 return col;
//             }



//         ENDCG
//     }
    
// }
// }
// 	Properties {
// 		_Color ("Tint", Color) = (0, 0, 0, 1)
// 		_MainTex ("Texture", 2D) = "white" {}
// 		_Smoothness ("Smoothness", Range(0, 1)) = 0
// 		_Metallic ("Metalness", Range(0, 1)) = 0
// 		[HDR] _Emission ("Emission", color) = (0,0,0)

// 		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
// 	}
// 	SubShader {
// 		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

//         //stencil operation
// 		Stencil{
// 			Ref [_StencilRef]
// 			Comp Equal
// 		}

// 		CGPROGRAM

// 		#pragma surface surf Standard fullforwardshadows
// 		#pragma target 3.0

// 		sampler2D _MainTex;
// 		fixed4 _Color;

// 		half _Smoothness;
// 		half _Metallic;
// 		half3 _Emission;

// 		struct Input {
// 			float2 uv_MainTex;
// 		};

// 		void surf (Input i, inout SurfaceOutputStandard o) {
// 			fixed4 col = tex2D(_MainTex, i.uv_MainTex);
// 			col *= _Color;
// 			o.Albedo = col.rgb;
// 			o.Metallic = _Metallic;
// 			o.Smoothness = _Smoothness;
// 			o.Emission = _Emission;
// 		}
// 		ENDCG
// 	}
// 	FallBack "Standard"
// }




