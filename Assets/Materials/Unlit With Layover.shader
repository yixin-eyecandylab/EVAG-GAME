// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - modifies the Alpha channel

Shader "Custom/Unlit With Layover" {

	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LayOverTex ("Alpha Blendes (RGBA)", 2D) = "white" {}
		_Color ("Blend Color", Color) = (1,1,1,0)
	}
	
	
    SubShader {
    	Tags { "Queue" = "Geometry" }
        Pass {
//   			Blend SrcAlpha OneMinusSrcAlpha
//            SetTexture [_MainTex] {
//              	constantColor [_Color]
//                combine constant * texture
//            }

            SetTexture [_MainTex] {
                combine texture
            }

            SetTexture [_LayOverTex] {
              	constantColor [_Color]
                combine constant * texture + previous
            }
        }
    }
    	
}

