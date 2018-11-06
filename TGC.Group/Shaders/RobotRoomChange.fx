float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))
static const int MAX_MATRICES = 26;
float4x3 bonesMatWorldArray[MAX_MATRICES];

float time = 0;

texture texDiffuseMap;
sampler2D baseMap =
sampler_state
{
	Texture = (texDiffuseMap);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

struct VS_INPUT
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
};

struct PS_INPUT
{
	float3 WorldPosition : TEXCOORD3;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
};

VS_OUTPUT vs_main (VS_INPUT Input)
{
	VS_OUTPUT Output;
	
	Output.Position = mul(Input.Position, matWorldViewProj);

	Output.Texcoord = Input.Texcoord;

	Output.Color = Input.Color;

	return (Output);
}

float4 ps_main( PS_INPUT input) : COLOR0
{
	float4 fvBaseColor = tex2D( baseMap, input.Texcoord );
	if (input.Texcoord.y < sin(time))
	{
		discard;
	}
	return 0.8*fvBaseColor + 0.2*input.Color;
}

technique RenderScene
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_main();
	}
}

