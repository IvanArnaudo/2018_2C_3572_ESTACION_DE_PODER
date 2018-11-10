float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))
static const int MAX_MATRICES = 26;
float4x3 bonesMatWorldArray[MAX_MATRICES];

float time = 0;
float4 robotPosition = 0;
float screen_w = 0;
float screen_h = 0;

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


struct PS_INPUT
{
	float3 WorldPosition : TEXCOORD3;
	float4 Color : COLOR0;
	float2 Texcoord : TEXCOORD0;
	float4 pos : TEXCOORD1;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float2 RealPos : TEXCOORD1;
    float4 Color : COLOR0;
};

VS_OUTPUT vs_main2(VS_INPUT Input)
{
    VS_OUTPUT Output;

    //if (-Input.Position.y < cos(time)*300)
    //{
	//	Input.Position.y =  Input.Position.y + 300;
    //}
  
    Output.RealPos = Input.Position;

	//Proyectar posicion
    Output.Position = mul(Input.Position, matWorldViewProj);

	//Propago las coordenadas de textura
    Output.Texcoord = Input.Texcoord;

	//Propago el color x vertice
    Output.Color = Input.Color;
    return (Output);
}

float frecuencia = 10;
//Pixel Shader
float4 ps_main(PS_INPUT Input) : COLOR0
{
	//float4 fvBaseColor = tex2D( baseMap, Input.Texcoord );
    //float y = Input.pos.y;
    //if (y % 15 > 15 * abs(sin(time)))
    //{
    //	discard;
    //}
    //return 0.8*fvBaseColor + 0.2*Input.Color;

    float2 newTex = Input.Texcoord;
    if (Input.Texcoord.x > 0.5)
    {
    	float k = screen_w/2;
    	float j = screen_h/2;
    	newTex = float2(floor(newTex.x * screen_w/2) / screen_w/2, floor(newTex.y*j)/j);
    }
    float4 fvBaseColor = tex2D( baseMap, newTex );
    return fvBaseColor;
}

technique RenderScene
{
	pass Pass_0
	{
		VertexShader = compile vs_3_0 vs_main2();
		PixelShader = compile ps_3_0 ps_main();
	}
}

