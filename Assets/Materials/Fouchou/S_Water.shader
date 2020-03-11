// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Water"
{
	Properties
	{
		_WaveSpeed("Wave Speed", Float) = 1
		_WaveDirection("Wave Direction", Vector) = (0,0,0,0)
		_WaveTile("Wave Tile", Float) = 0
		_WaveStretch("Wave Stretch", Vector) = (0.2,0.01,0,0)
		_Tesselation("Tesselation", Float) = 0
		_WaveHeight("WaveHeight", Float) = 1
		_Smoothness("Smoothness", Float) = 0.9
		_TopColor("TopColor", Color) = (0,0,0,0)
		_WaterColor("WaterColor", Color) = (0,0,0,0)
		_EdgeDistance("Edge Distance", Float) = 1
		_EdgePower("Edge Power", Range( 0 , 1)) = 1
		_NormalMap("Normal Map", 2D) = "white" {}
		_PanDirection("Pan Direction", Vector) = (1,0,0,0)
		_NormalSpeed("Normal Speed", Float) = 0
		_PanDirection2("Pan Direction2", Vector) = (-1,0,0,0)
		_NormalStrength("Normal Strength", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float _WaveHeight;
		uniform float _WaveSpeed;
		uniform float2 _WaveDirection;
		uniform float2 _WaveStretch;
		uniform float _WaveTile;
		uniform sampler2D _NormalMap;
		uniform float _NormalStrength;
		uniform float2 _PanDirection;
		uniform float _NormalSpeed;
		uniform float2 _PanDirection2;
		uniform float4 _WaterColor;
		uniform float4 _TopColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _EdgeDistance;
		uniform float _EdgePower;
		uniform float _Smoothness;
		uniform float _Tesselation;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			float4 temp_cast_3 = (_Tesselation).xxxx;
			return temp_cast_3;
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float temp_output_9_0 = ( _Time.y * _WaveSpeed );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult11 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile12 = appendResult11;
			float4 WaveTIleUV25 = ( ( WorldSpaceTile12 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_9_0 * _WaveDirection + WaveTIleUV25.xy);
			float simplePerlin2D1 = snoise( panner3 );
			simplePerlin2D1 = simplePerlin2D1*0.5 + 0.5;
			float2 panner26 = ( temp_output_9_0 * _WaveDirection + ( WaveTIleUV25 * float4( 0,0,0,0 ) ).xy);
			float simplePerlin2D27 = snoise( panner26 );
			simplePerlin2D27 = simplePerlin2D27*0.5 + 0.5;
			float temp_output_31_0 = ( simplePerlin2D1 + simplePerlin2D27 );
			float3 WaveHeight38 = ( ( float3(0,1,0) * _WaveHeight ) * temp_output_31_0 );
			v.vertex.xyz += WaveHeight38;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner70 = ( 1.0 * _Time.y * ( _PanDirection * _NormalSpeed ) + float2( 0,0 ));
			float2 uv_TexCoord85 = i.uv_texcoord + panner70;
			float2 panner71 = ( 1.0 * _Time.y * ( _PanDirection2 * ( _NormalSpeed * 3.0 ) ) + float2( 0,0 ));
			float2 uv_TexCoord86 = i.uv_texcoord + panner71;
			float3 Normals81 = BlendNormals( UnpackScaleNormal( tex2D( _NormalMap, uv_TexCoord85 ), _NormalStrength ) , UnpackScaleNormal( tex2D( _NormalMap, uv_TexCoord86 ), _NormalStrength ) );
			o.Normal = Normals81;
			float temp_output_9_0 = ( _Time.y * _WaveSpeed );
			float3 ase_worldPos = i.worldPos;
			float4 appendResult11 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile12 = appendResult11;
			float4 WaveTIleUV25 = ( ( WorldSpaceTile12 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_9_0 * _WaveDirection + WaveTIleUV25.xy);
			float simplePerlin2D1 = snoise( panner3 );
			simplePerlin2D1 = simplePerlin2D1*0.5 + 0.5;
			float2 panner26 = ( temp_output_9_0 * _WaveDirection + ( WaveTIleUV25 * float4( 0,0,0,0 ) ).xy);
			float simplePerlin2D27 = snoise( panner26 );
			simplePerlin2D27 = simplePerlin2D27*0.5 + 0.5;
			float temp_output_31_0 = ( simplePerlin2D1 + simplePerlin2D27 );
			float WavePattern34 = temp_output_31_0;
			float clampResult51 = clamp( WavePattern34 , 0.0 , 1.0 );
			float4 lerpResult48 = lerp( _WaterColor , _TopColor , clampResult51);
			float4 Albedo52 = lerpResult48;
			o.Albedo = Albedo52.rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth55 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth55 = abs( ( screenDepth55 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeDistance ) );
			float clampResult62 = clamp( ( ( 1.0 - distanceDepth55 ) * _EdgePower ) , 0.0 , 0.5 );
			float Edge60 = clampResult62;
			float3 temp_cast_4 = (Edge60).xxx;
			o.Emission = temp_cast_4;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17800
392;73;1036;601;2652.468;-74.91432;1.662534;True;False
Node;AmplifyShaderEditor.CommentaryNode;13;-4496.479,-1509.786;Inherit;False;628.9244;309;World Space UVs;3;10;11;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-4446.479,-1458.396;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;11;-4255.553,-1458.786;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;29;-3814.696,-1498.768;Inherit;False;1599.979;618.2205;Wave UVs & Tile;11;36;35;24;23;25;19;17;20;16;18;38;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-4091.553,-1459.786;Inherit;True;WorldSpaceTile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;18;-3702.893,-1359.907;Float;False;Property;_WaveStretch;Wave Stretch;3;0;Create;True;0;0;False;0;0.2,0.01;0.15,0.02;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;16;-3764.696,-1446.205;Inherit;False;12;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-3470.792,-1448.328;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-3493.053,-1332.485;Float;False;Property;_WaveTile;Wave Tile;2;0;Create;True;0;0;False;0;0;1.63;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-3271.895,-1445.907;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;40;-4494.522,-832.6227;Inherit;False;1989.705;822.1019;Wave Pattern;13;32;33;26;27;31;34;3;1;5;7;28;9;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-3049.228,-1448.768;Inherit;False;WaveTIleUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-4435.691,-247.1978;Float;False;Property;_WaveSpeed;Wave Speed;0;0;Create;True;0;0;False;0;1;0.64;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;5;-4444.522,-355.7552;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;32;-3963.337,-155.0065;Inherit;False;25;WaveTIleUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-4259.229,-264.5208;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;4;-4224.751,-537.5864;Float;True;Property;_WaveDirection;Wave Direction;1;0;Create;True;0;0;False;0;0,0;1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;28;-4233.042,-782.6227;Inherit;True;25;WaveTIleUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-3751.849,-156.0136;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2724.455,158.3066;Inherit;False;2837.992;932.0205;Normal;21;66;65;69;68;67;72;70;71;77;74;76;75;73;45;64;80;78;81;63;85;86;;0,0.6117647,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1979.959,392.6233;Inherit;False;Property;_NormalSpeed;Normal Speed;14;0;Create;True;0;0;False;0;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;26;-3539.817,-312.6393;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;3;-3551.669,-558.3032;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;72;-1972.982,208.3065;Float;False;Property;_PanDirection;Pan Direction;13;0;Create;True;0;0;False;0;1,0;1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;73;-1654.009,929.3267;Float;False;Property;_PanDirection2;Pan Direction2;15;0;Create;True;0;0;False;0;-1,0;-1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1723.798,762.5646;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;84;-554.2808,-1257.565;Inherit;False;1348.517;429.0016;Camera Depth;7;56;55;59;57;58;62;60;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-3319.13,-564.4;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;27;-3309.737,-302.1311;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-1427.147,936.2798;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1727.01,347.2948;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-504.2808,-1193.394;Float;False;Property;_EdgeDistance;Edge Distance;9;0;Create;True;0;0;False;0;1;0.006;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-2997.221,-534.4751;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;54;-1796.252,-1241.061;Inherit;False;988.5995;591.6993;Color;6;46;47;48;49;51;52;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DepthFade;55;-254.2538,-1194.739;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-2728.817,-524.447;Float;False;WavePattern;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;71;-1222.748,911.4434;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.2,0.2;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;70;-1400.802,321.843;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0.5,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;85;-1054.001,291.9349;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;80;-1053.978,567.7972;Float;True;Property;_NormalStrength;Normal Strength;16;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;63;-1296.501,599.6625;Inherit;True;Property;_NormalMap;Normal Map;11;0;Create;True;0;0;False;0;None;d01553631fc28784682c500d948cc711;True;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector3Node;23;-3184.669,-1177.54;Float;False;Constant;_WaveUp;Wave Up;4;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;57;32.49602,-1193.047;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-1746.252,-808.8208;Inherit;False;34;WavePattern;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-22.64184,-1087.563;Float;True;Property;_EdgePower;Edge Power;10;0;Create;True;0;0;False;0;1;0.341;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-3186.445,-1030.83;Float;False;Property;_WaveHeight;WaveHeight;5;0;Create;True;0;0;False;0;1;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;86;-933.9554,873.6077;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-708.1817,339.1765;Inherit;True;Property;_TextureSample1;Texture Sample 1;7;0;Create;True;0;0;False;0;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;64;-677.4749,810.8276;Inherit;True;Property;_TextureSample0;Texture Sample 0;7;0;Create;True;0;0;False;0;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;47;-1622.633,-1191.061;Float;False;Property;_WaterColor;WaterColor;8;0;Create;True;0;0;False;0;0,0,0,0;0.1012371,0.1645839,0.2358491,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;46;-1621.146,-988.1165;Float;False;Property;_TopColor;TopColor;7;0;Create;True;0;0;False;0;0,0,0,0;0.3304557,0.5900795,0.8867924,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2980.671,-1164.092;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;51;-1530.682,-808.3616;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;211.3582,-1199.563;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;78;-397.1444,490.4034;Inherit;True;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;62;373.5143,-1207.565;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2670.746,-1044.63;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;48;-1269.633,-1013.061;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;52;-1031.653,-1011.199;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;-80.92487,499.7874;Float;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;38;-2505.372,-1021.437;Inherit;False;WaveHeight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;570.2362,-1202.965;Float;True;Edge;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;697.1413,-27.28774;Inherit;False;81;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;-2686.831,356.878;Inherit;True;12;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-2369.34,624.4036;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;53;693.764,-107.2644;Inherit;False;52;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;694.8312,54.13358;Inherit;False;60;Edge;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;658.1657,210.9354;Inherit;True;38;WaveHeight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;41;699.601,133.8602;Float;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;False;0;0.9;0.37;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-2613.455,565.0147;Float;False;Property;_NormalTile;NormalTile;12;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;705.0432,405.9937;Float;False;Property;_Tesselation;Tesselation;4;0;Create;True;0;0;False;0;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-2165.202,601.0731;Inherit;True;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-2317.455,415.0148;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1098.459,-1.533578;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;S_Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;10;1
WireConnection;11;1;10;3
WireConnection;12;0;11;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;19;0;17;0
WireConnection;19;1;20;0
WireConnection;25;0;19;0
WireConnection;9;0;5;0
WireConnection;9;1;7;0
WireConnection;33;0;32;0
WireConnection;26;0;33;0
WireConnection;26;2;4;0
WireConnection;26;1;9;0
WireConnection;3;0;28;0
WireConnection;3;2;4;0
WireConnection;3;1;9;0
WireConnection;76;0;75;0
WireConnection;1;0;3;0
WireConnection;27;0;26;0
WireConnection;77;0;73;0
WireConnection;77;1;76;0
WireConnection;74;0;72;0
WireConnection;74;1;75;0
WireConnection;31;0;1;0
WireConnection;31;1;27;0
WireConnection;55;0;56;0
WireConnection;34;0;31;0
WireConnection;71;2;77;0
WireConnection;70;2;74;0
WireConnection;85;1;70;0
WireConnection;57;0;55;0
WireConnection;86;1;71;0
WireConnection;45;0;63;0
WireConnection;45;1;85;0
WireConnection;45;5;80;0
WireConnection;64;0;63;0
WireConnection;64;1;86;0
WireConnection;64;5;80;0
WireConnection;24;0;23;0
WireConnection;24;1;35;0
WireConnection;51;0;49;0
WireConnection;58;0;57;0
WireConnection;58;1;59;0
WireConnection;78;0;45;0
WireConnection;78;1;64;0
WireConnection;62;0;58;0
WireConnection;36;0;24;0
WireConnection;36;1;31;0
WireConnection;48;0;47;0
WireConnection;48;1;46;0
WireConnection;48;2;51;0
WireConnection;52;0;48;0
WireConnection;81;0;78;0
WireConnection;38;0;36;0
WireConnection;60;0;62;0
WireConnection;68;0;67;0
WireConnection;69;0;66;0
WireConnection;69;1;68;0
WireConnection;66;0;65;0
WireConnection;66;1;67;0
WireConnection;0;0;53;0
WireConnection;0;1;82;0
WireConnection;0;2;61;0
WireConnection;0;4;41;0
WireConnection;0;11;39;0
WireConnection;0;14;21;0
ASEEND*/
//CHKSM=95911083761866A927693C9C25D00E406037E830