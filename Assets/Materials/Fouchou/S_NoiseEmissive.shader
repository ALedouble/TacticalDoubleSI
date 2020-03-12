// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_NoiseEmissive"
{
	Properties
	{
		_WaterBump1("WaterBump 1", 2D) = "white" {}
		_NoiseColor("NoiseColor", Color) = (0.2627451,0,1,0)
		_BaseColor("BaseColor", Color) = (0.8705883,0,1,1)
		_NoiseSpeed("Noise Speed", Vector) = (0.2,0.2,0,0)
		_MulitplyColor("MulitplyColor", Vector) = (3,3,3,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _BaseColor;
		uniform sampler2D _WaterBump1;
		uniform float2 _NoiseSpeed;
		uniform float4 _NoiseColor;
		uniform float3 _MulitplyColor;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _BaseColor.rgb;
			float2 panner5 = ( 1.0 * _Time.y * _NoiseSpeed + float2( 0,0 ));
			float2 uv_TexCoord10 = i.uv_texcoord + panner5;
			o.Emission = ( tex2D( _WaterBump1, uv_TexCoord10 ) * ( _NoiseColor * float4( _MulitplyColor , 0.0 ) ) ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
352;206;1036;601;1507.945;77.67459;1;True;False
Node;AmplifyShaderEditor.Vector2Node;11;-1651.477,44.14722;Float;False;Property;_NoiseSpeed;Noise Speed;3;0;Create;True;0;0;False;0;0.2,0.2;0.2,0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;5;-1451.449,23.57565;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.25,0.25;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1180.705,-20.17719;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-1100.75,158.7288;Float;False;Property;_NoiseColor;NoiseColor;1;0;Create;True;0;0;False;0;0.2627451,0,1,0;0.1984921,0,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;12;-1059.477,338.1472;Float;False;Property;_MulitplyColor;MulitplyColor;4;0;Create;True;0;0;False;0;3,3,3;3,3,3;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;1;-959.6769,-52.32632;Inherit;True;Property;_WaterBump1;WaterBump 1;0;0;Create;True;0;0;False;0;-1;a1be982624122584d90046922d9300df;a1be982624122584d90046922d9300df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-872.4768,159.1472;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-618.0446,47.6917;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;4;-543.9916,-226.5593;Float;False;Property;_BaseColor;BaseColor;2;0;Create;True;0;0;False;0;0.8705883,0,1,1;0.486094,0,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;S_NoiseEmissive;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;5;2;11;0
WireConnection;10;1;5;0
WireConnection;1;1;10;0
WireConnection;13;0;2;0
WireConnection;13;1;12;0
WireConnection;3;0;1;0
WireConnection;3;1;13;0
WireConnection;0;0;4;0
WireConnection;0;2;3;0
ASEEND*/
//CHKSM=A98F22CBB440C6EACBD9B8F741062D3CA0E0E53B