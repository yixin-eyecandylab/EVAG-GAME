Shader "Unlit/BillboardShader Transparent" 
{
   Properties {
      _MainTex ("Texture Image", 2D) = "white" {}
      _Color ("Color", Color) = (1,1,1,0)
   }
   SubShader {
    //  Tags { "Queue"="Transparent" }
      Pass {   

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 

         // User-specified uniforms            
         uniform sampler2D _MainTex;
         uniform float4 _Color;        
 
         struct vertexInput {
            float4 vertex : POSITION;
            float4 tex : TEXCOORD0;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 tex : TEXCOORD0;
         };
 
         vertexOutput vert(vertexInput input) 
         {
            vertexOutput output;

//            output.pos = mul(UNITY_MATRIX_P, 
//              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
//              - float4(input.vertex.x, input.vertex.y, 0.0, 0.0));

			float scaleX = length(mul(_Object2World, float4(1.0, 0.0, 0.0, 0.0)));
            float scaleY = length(mul(_Object2World, float4(0.0, 1.0, 0.0, 0.0)));
            output.pos = mul(UNITY_MATRIX_P, 
              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              - float4(input.vertex.x * scaleX, input.vertex.y * scaleY, 0.0, -0.5));

  
            output.tex = input.tex;
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
//            return tex2D(_MainTex, float2(input.tex.xy));   
            //return float4(1.0, 0.0, 0.0, 1.0);
            
            float4 texCol = tex2D(_MainTex, float2(input.tex.xy));   
            return float4(_Color.x, _Color.y, _Color.z, texCol.w);
//			return _Color
         }


         ENDCG

           Blend SrcAlpha OneMinusSrcAlpha

//            SetTexture [_MainTex] {
//              	constantColor [_Color]
//                combine constant * texture
//            }

            ZWrite Off
   //         ZTest Always
   
      }
   }
}

//Shader "Custom/Unlit With Layover" {
//
//	Properties
//	{
//		_MainTex ("Base (RGB)", 2D) = "white" {}
//		_LayOverTex ("Alpha Blendes (RGBA)", 2D) = "white" {}
//		_Color ("Blend Color", Color) = (1,1,1,0)
//	}
//	
//	
//    SubShader {
//    	Tags { "Queue" = "Geometry" }
//        Pass {
////   			Blend SrcAlpha OneMinusSrcAlpha
////            SetTexture [_MainTex] {
////              	constantColor [_Color]
////                combine constant * texture
////            }
//
//            SetTexture [_MainTex] {
//                combine texture
//            }
//
//            SetTexture [_LayOverTex] {
//              	constantColor [_Color]
//                combine constant * texture + previous
//            }
//        }
//    }
//    	
//}
//
