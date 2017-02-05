Shader "Custom/BasicWorldspaceUV" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo Walls (RGB)", 2D) = "white" {}
		_MainTexTop ("Albedo Top (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Scale ("ScaleWalls", Range(0,10)) = 0.0
		_Scale2 ("ScaleFloor", Range(0,10)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MainTexTop;

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		half _Scale;
		half _Scale2;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float2 UV;

		    if(abs(IN.worldNormal.x)>0.5)
		    {
		        UV = IN.worldPos.zy; // side
		        c = tex2D(_MainTex, UV* _Scale) * _Color; // use WALL texture
		    }
		    else if(abs(IN.worldNormal.z)>0.5)
		    {
		        UV = IN.worldPos.xy; // front
		        c = tex2D(_MainTex, UV* _Scale) * _Color; // use WALL texture
		    }
		    else
		    {
		        UV = IN.worldPos.xz; // top
		        c = tex2D(_MainTexTop, UV* _Scale2) * _Color; // use FLR texture
		    }

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}