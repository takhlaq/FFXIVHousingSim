// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Silent/FF14/TexBlend"
{
	Properties
	{
		[Toggle(_)]_DisableBlending("Disable Blending", Float) = 1
		[NoScaleOffset]_Albedo0("Albedo 0", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap0("Normal Map 0", 2D) = "bump" {}
		[NoScaleOffset]_Metallic0("Metallic 0", 2D) = "black" {}
		[NoScaleOffset]_Albedo1("Albedo 1", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap1("Normal Map 1", 2D) = "bump" {}
		[NoScaleOffset]_Metallic1("Metallic 1", 2D) = "black" {}
		_EmissionPow("Emission Pow", Range( 0 , 100)) = 0
		_Texture0ScaleOffset("Texture 0 Scale/Offset", Vector) = (1,1,0,0)
		[Toggle(_)]_UseXYZW("UseXYZW", Float) = 0
		_Metalness("Metalness", Range( 0 , 1)) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.8
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_tex4coord;
			float4 vertexColor : COLOR;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalMap0;
		uniform float4 _Texture0ScaleOffset;
		uniform sampler2D _NormalMap1;
		uniform float _UseXYZW;
		uniform float _DisableBlending;
		uniform sampler2D _Albedo0;
		uniform sampler2D _Albedo1;
		uniform sampler2D _Metallic0;
		uniform sampler2D _Metallic1;
		uniform float _EmissionPow;
		uniform float _Metalness;
		uniform float _Smoothness;


		float PerceptualSmoothnessToRoughness184( float perceptualSmoothness )
		{
			return (1.0 - perceptualSmoothness) * (1.0 - perceptualSmoothness);
		}


		float RoughnessToPerceptualSmoothness194( float roughness )
		{
			return 1.0 - sqrt(roughness);
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult165 = (float2(_Texture0ScaleOffset.x , _Texture0ScaleOffset.y));
			float2 appendResult166 = (float2(_Texture0ScaleOffset.z , _Texture0ScaleOffset.w));
			float4 uv_TexCoord50 = i.uv_tex4coord;
			uv_TexCoord50.xy = i.uv_tex4coord.xy * appendResult165 + appendResult166;
			float2 temp_output_201_0 = (uv_TexCoord50).xy;
			float2 secondUVset200 = lerp(temp_output_201_0,(uv_TexCoord50).zw,_UseXYZW);
			float BlendValue60 = lerp(i.vertexColor.a,0.0,_DisableBlending);
			float3 lerpResult58 = lerp( UnpackNormal( tex2D( _NormalMap0, temp_output_201_0 ) ) , UnpackNormal( tex2D( _NormalMap1, secondUVset200 ) ) , BlendValue60);
			o.Normal = lerpResult58;
			float4 lerpResult84 = lerp( tex2D( _Albedo0, temp_output_201_0 ) , tex2D( _Albedo1, secondUVset200 ) , BlendValue60);
			float4 AlbedoFinal71 = lerpResult84;
			o.Albedo = AlbedoFinal71.rgb;
			float4 lerpResult59 = lerp( tex2D( _Metallic0, temp_output_201_0 ) , tex2D( _Metallic1, secondUVset200 ) , BlendValue60);
			float4 break64 = lerpResult59;
			o.Emission = ( break64.a * AlbedoFinal71 * _EmissionPow ).rgb;
			o.Metallic = ( break64.b * _Metalness );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 temp_output_142_0 = ddx( ase_worldNormal );
			float dotResult144 = dot( temp_output_142_0 , temp_output_142_0 );
			float3 temp_output_143_0 = ddy( ase_worldNormal );
			float dotResult145 = dot( temp_output_143_0 , temp_output_143_0 );
			float baseSmoothness182 = ( _Smoothness * break64.r );
			float perceptualSmoothness184 = baseSmoothness182;
			float localPerceptualSmoothnessToRoughness184 = PerceptualSmoothnessToRoughness184( perceptualSmoothness184 );
			float roughness194 = sqrt( saturate( ( min( ( 2.0 * ( ( dotResult144 + dotResult145 ) * 0.25 ) ) , ( 0.5 * 0.5 ) ) + ( localPerceptualSmoothnessToRoughness184 * localPerceptualSmoothnessToRoughness184 ) ) ) );
			float localRoughnessToPerceptualSmoothness194 = RoughnessToPerceptualSmoothness194( roughness194 );
			float geomSmoothness150 = localRoughnessToPerceptualSmoothness194;
			o.Smoothness = geomSmoothness150;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xyzw = customInputData.uv_tex4coord;
				o.customPack1.xyzw = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_tex4coord = IN.customPack1.xyzw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16700
1311;92;2529;1366;1366.445;-481.6977;1;True;False
Node;AmplifyShaderEditor.Vector4Node;164;-2829.875,93.52783;Float;False;Property;_Texture0ScaleOffset;Texture 0 Scale/Offset;12;0;Create;True;0;0;False;0;1,1,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;166;-2579.875,160.5278;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;165;-2575.875,77.52783;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;50;-2273.639,-21.31805;Float;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;202;-1977.843,184.4764;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;201;-1967.843,-63.52362;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-1268.933,-863.2946;Float;False;Constant;_Float1;Float 1;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;88;-1284,-1097.265;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;199;-1631.843,36.47638;Float;False;Property;_UseXYZW;UseXYZW;15;0;Create;True;0;0;False;0;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ToggleSwitchNode;153;-1016.202,-976.2184;Float;False;Property;_DisableBlending;Disable Blending;0;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;140;-4166.135,-1378.244;Float;False;1972;457.4999;For specular AA;17;150;145;144;179;180;183;184;185;187;186;188;189;190;191;192;193;194;Geometric Roughness Factor;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;200;-1366.843,18.47638;Float;False;secondUVset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;-282.9399,-972.7994;Float;False;BlendValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;187;-4137.426,-1314.49;Float;False;878;274;variance;4;181;141;142;143;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;4;-1147.296,685.766;Float;True;Property;_Metallic0;Metallic 0;3;1;[NoScaleOffset];Create;True;0;0;False;0;None;fdb6f13f1ea8977458a9dd4d4111da10;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;62;-1019.136,1096.063;Float;False;60;BlendValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;57;-1142.16,895.0431;Float;True;Property;_Metallic1;Metallic 1;6;1;[NoScaleOffset];Create;True;0;0;False;0;None;361301307ca323f4bbf9add265bbdf98;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;141;-4125.042,-1202.095;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;59;-687.0394,843.3876;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DdxOpNode;142;-3920,-1264;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;204;-518.9293,520.8826;Float;False;Property;_Smoothness;Smoothness;17;0;Create;True;0;0;False;0;0.8;0.8;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;64;-499.6379,819.5853;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DdyOpNode;143;-3920,-1136;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;206;-166.5501,694.9493;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;145;-3776,-1136;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;144;-3776,-1264;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;182;-10.33599,682.7151;Float;False;baseSmoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;181;-3648,-1136;Float;False;Constant;_screenSpaceVariance;screenSpaceVariance;15;0;Create;True;0;0;False;0;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;179;-3648,-1264;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;183;-3785.426,-1022.49;Float;False;182;baseSmoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-3408,-1264;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;189;-3232,-1184;Float;False;Constant;_threshold;threshold;15;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;184;-3555.426,-1020.49;Float;False;return (1.0 - perceptualSmoothness) * (1.0 - perceptualSmoothness)@;1;False;1;True;perceptualSmoothness;FLOAT;0;In;;Float;False;PerceptualSmoothnessToRoughness;True;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;190;-3056,-1184;Float;False;2;2;0;FLOAT;2;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-3213.426,-1281.49;Float;False;2;2;0;FLOAT;2;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;186;-2912,-1280;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;192;-3198.426,-1054.49;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;191;-2784,-1152;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-827.4937,-500.5341;Float;True;Property;_Albedo0;Albedo 0;1;1;[NoScaleOffset];Create;True;0;0;False;0;None;32eb5aad289076b4faa28f9253c6353c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;87;-459.6017,-335.7636;Float;False;60;BlendValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;49;-832.2064,-297.8654;Float;True;Property;_Albedo1;Albedo 1;4;1;[NoScaleOffset];Create;True;0;0;False;0;None;7ec306354d739cf4fa7dcd60d3342007;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;185;-2664.426,-1177.49;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;84;-373.9803,-550.6643;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SqrtOpNode;193;-2563.426,-1022.49;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;194;-2558.426,-1312.49;Float;False;return 1.0 - sqrt(roughness)@;1;False;1;True;roughness;FLOAT;0;In;;Float;False;RoughnessToPerceptualSmoothness;True;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;302.3309,-520.1566;Float;False;AlbedoFinal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;73;-486.852,1019.953;Float;False;71;AlbedoFinal;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;135;-602.4876,1322.502;Float;False;737;215;Lightmap Hack;5;134;137;133;132;131;Multiplies the emission when time is zero.;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;56;-855.7323,312.0049;Float;True;Property;_NormalMap1;Normal Map 1;5;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;309fb2e5196fa254986834cd4cbb8268;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;205;-509.3766,601.5479;Float;False;Property;_Metalness;Metalness;16;0;Create;True;0;0;False;0;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-851.8496,104.7894;Float;True;Property;_NormalMap0;Normal Map 0;2;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;920bc6b3e5d373c479d1906c25f6641d;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;6;-806.3942,1157.066;Float;False;Property;_EmissionPow;Emission Pow;7;0;Create;True;0;0;False;0;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-568.9885,387.5907;Float;False;60;BlendValue;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-2450.139,-1150.643;Float;False;geomSmoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;86;-1281.134,-757.3568;Float;True;Property;_TextureSample0;Texture Sample 0;2;1;[NoScaleOffset];Create;True;0;0;False;0;None;b28045cdce9535548bc595a44521ccf3;True;0;True;bump;Auto;False;Instance;3;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DdxOpNode;171;-3722.506,-1818.693;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;58;-499.4165,206.7442;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-165.4888,600.4863;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;150.8783,913.5651;Float;True;3;3;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;159;-266.329,-25.14258;Float;False;Constant;_Float3;Float 3;13;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;54;-485.2009,-242.7999;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;176;-3025.507,-1833.693;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;113;-4072.358,268.1367;Float;True;Property;_ParallaxMapSampler;Parallax MapT;8;1;[NoScaleOffset];Create;False;0;0;False;0;None;1a3cdec6fb04a0a4aa32fb62469335ae;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ParallaxOcclusionMappingNode;117;-3373.036,8.925404;Float;False;2;8;False;-1;16;False;-1;2;0.02;0.5;False;1,1;False;0,0;Texture2D;7;0;FLOAT2;0,0;False;1;SAMPLER2D;;False;2;FLOAT;0.02;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT2;0,0;False;6;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;126;-4288.451,-197.8882;Float;True;Property;_ParallaxMap;Parallax Map;10;0;Create;False;0;0;False;0;None;None;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.OneMinusNode;178;-2564.507,-1809.693;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;124;-3929.613,84.92596;Float;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.IntNode;197;-2117.161,749.5929;Float;False;Property;_Int0;Int 0;14;1;[Enum];Create;True;3;None;0;XYZW;1;UV1UV2;2;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;83;-210.9803,-241.6643;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;95;-417.7131,-63.52338;Float;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;131;-553,1417;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;138;-314,1176;Float;False;Property;_UNITY_PASS_META;UNITY_PASS_META;14;0;Fetch;True;0;0;False;0;0;0;0;False;UNITY_PASS_META;Toggle;2;Key0;Key1;Fetch;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-511,1216;Float;False;2;2;0;FLOAT;5;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;41.02002,-428.6111;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;129;-99.64136,-160.4139;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;192.3032,113.6543;Float;False;150;geomSmoothness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;174;-3458.506,-1728.693;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;128;-3095.409,-55.10965;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;157;89.67078,-25.14258;Float;False;Lerp White To;-1;;1;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-3608.064,250.9656;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.DdyOpNode;172;-3716.506,-1707.693;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;109;-2266.998,358.0495;Float;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;170;-4072.506,-1815.693;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-3595.047,134.0943;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;118;-3191.365,370.4116;Float;False;Property;_DisableParallaxEffect;Disable Parallax Effect;9;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;383.4684,-44.51451;Float;False;71;AlbedoFinal;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;167;-2576.663,353.3157;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;175;-3200.507,-1843.693;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;123;-3359.883,201.6566;Float;False;Constant;_Float2;Float 2;11;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;132;-393,1417;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;162;-2268.875,176.5278;Float;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;502.3922,-261.434;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;173;-3453.506,-1848.693;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;169;-2826.663,286.3158;Float;False;Property;_Texture1ScaleOffset;Texture 1 Scale/Offset;13;0;Create;True;0;0;False;0;1,1,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-123,1394;Float;False;2;2;0;FLOAT;10;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;133;-265,1417;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;68;-1919.885,-274.5865;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ParallaxOffsetHlpNode;110;-3376.992,287.6381;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.005;False;2;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;125;-3661.683,-160.1119;Float;False;0;126;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;130;43.6311,-271.7611;Float;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-81.32898,-39.14258;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-862.3467,-748.5497;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;168;-2572.663,270.3158;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;209;-190.98,-368.6111;Float;False;Lerp White To;-1;;2;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;111;-3700.334,362.8203;Float;False;Tangent;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;101;-688.2937,-742.3562;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;177;-2740.507,-1797.693;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.3333333;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;127;-3350.692,-163.1454;Float;False;0;126;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;137;5,1394;Float;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;156;-446.3292,73.85742;Float;False;Property;_VertexColourPower;Vertex Colour Power;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;633.5719,-61.98408;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Silent/FF14/TexBlend;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;166;0;164;3
WireConnection;166;1;164;4
WireConnection;165;0;164;1
WireConnection;165;1;164;2
WireConnection;50;0;165;0
WireConnection;50;1;166;0
WireConnection;202;0;50;0
WireConnection;201;0;50;0
WireConnection;199;0;201;0
WireConnection;199;1;202;0
WireConnection;153;0;88;4
WireConnection;153;1;122;0
WireConnection;200;0;199;0
WireConnection;60;0;153;0
WireConnection;4;1;201;0
WireConnection;57;1;200;0
WireConnection;59;0;4;0
WireConnection;59;1;57;0
WireConnection;59;2;62;0
WireConnection;142;0;141;0
WireConnection;64;0;59;0
WireConnection;143;0;141;0
WireConnection;206;0;204;0
WireConnection;206;1;64;0
WireConnection;145;0;143;0
WireConnection;145;1;143;0
WireConnection;144;0;142;0
WireConnection;144;1;142;0
WireConnection;182;0;206;0
WireConnection;179;0;144;0
WireConnection;179;1;145;0
WireConnection;180;0;179;0
WireConnection;180;1;181;0
WireConnection;184;0;183;0
WireConnection;190;0;189;0
WireConnection;190;1;189;0
WireConnection;188;1;180;0
WireConnection;186;0;188;0
WireConnection;186;1;190;0
WireConnection;192;0;184;0
WireConnection;192;1;184;0
WireConnection;191;0;186;0
WireConnection;191;1;192;0
WireConnection;2;1;201;0
WireConnection;49;1;200;0
WireConnection;185;0;191;0
WireConnection;84;0;2;0
WireConnection;84;1;49;0
WireConnection;84;2;87;0
WireConnection;193;0;185;0
WireConnection;194;0;193;0
WireConnection;71;0;84;0
WireConnection;56;1;200;0
WireConnection;3;1;201;0
WireConnection;150;0;194;0
WireConnection;86;1;201;0
WireConnection;171;0;170;0
WireConnection;58;0;3;0
WireConnection;58;1;56;0
WireConnection;58;2;61;0
WireConnection;207;0;64;2
WireConnection;207;1;205;0
WireConnection;5;0;64;3
WireConnection;5;1;73;0
WireConnection;5;2;6;0
WireConnection;176;0;175;0
WireConnection;117;0;125;0
WireConnection;117;1;126;0
WireConnection;117;3;124;0
WireConnection;178;0;177;0
WireConnection;83;0;84;0
WireConnection;83;1;54;0
WireConnection;138;1;6;0
WireConnection;138;0;139;0
WireConnection;139;1;6;0
WireConnection;208;0;84;0
WireConnection;208;1;209;0
WireConnection;129;0;83;0
WireConnection;129;1;95;0
WireConnection;174;0;172;0
WireConnection;174;1;172;0
WireConnection;128;0;117;0
WireConnection;128;1;127;0
WireConnection;157;1;161;0
WireConnection;120;0;113;3
WireConnection;172;0;170;0
WireConnection;109;0;168;0
WireConnection;109;1;167;0
WireConnection;115;0;113;3
WireConnection;118;1;110;0
WireConnection;118;0;123;0
WireConnection;167;0;169;3
WireConnection;167;1;169;4
WireConnection;175;0;173;0
WireConnection;175;1;174;0
WireConnection;132;0;131;0
WireConnection;162;0;168;0
WireConnection;162;1;167;0
WireConnection;155;0;157;0
WireConnection;173;0;171;0
WireConnection;173;1;171;0
WireConnection;134;1;133;0
WireConnection;133;0;132;0
WireConnection;68;0;50;1
WireConnection;68;1;50;2
WireConnection;110;0;115;0
WireConnection;110;2;111;0
WireConnection;130;0;129;0
WireConnection;161;0;54;0
WireConnection;161;1;159;0
WireConnection;121;0;153;0
WireConnection;121;1;86;4
WireConnection;168;0;169;1
WireConnection;168;1;169;2
WireConnection;209;1;54;0
WireConnection;209;2;156;0
WireConnection;101;0;121;0
WireConnection;177;0;176;0
WireConnection;137;1;134;0
WireConnection;0;0;72;0
WireConnection;0;1;58;0
WireConnection;0;2;5;0
WireConnection;0;3;207;0
WireConnection;0;4;151;0
ASEEND*/
//CHKSM=64CC0E0EDB78644F998080D72C1B5A4277708B91