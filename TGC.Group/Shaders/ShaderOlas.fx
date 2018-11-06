
/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; 
float4x4 matWorldView;
float4x4 matWorldViewProj;
float4x4 matInverseTransposeWorld;

float screen_dx;
float screen_dy;

texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

float time = 0;

/**************************************************************************************/
/* Olas */
/**************************************************************************************/

//Input Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

//Output Vertex Shader (Input Pixel Shader)
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float2 RealPos : TEXCOORD1;
    float4 Color : COLOR0;
};

//Vertex Shader: Proyecta la posicion a pantalla y deja lo demas igual.
VS_OUTPUT vs_main(VS_INPUT Input)
{
    VS_OUTPUT Output;
    Output.RealPos = Input.Position;

    Output.Position = mul(Input.Position, matWorldViewProj);
   
    Output.Texcoord = Input.Texcoord;

    Output.Color = Input.Color;

    return (Output);
}

float frecuencia = 8;
float factorY = 10;
float factorX = -10;

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
	
    float y = Input.Texcoord.y * screen_dy +  (3 * cos(factorY)) * sin(Input.Texcoord.y * time * frecuencia);
    Input.Texcoord.y = y / screen_dy;
	
	float x = Input.Texcoord.x * screen_dx +  (3 * sin(factorX)) * cos(Input.Texcoord.x * time * frecuencia);
    Input.Texcoord.x = x / screen_dx;
	
	float4 ret = tex2D(diffuseMap, Input.Texcoord);
			
    return ret;
}

// ------------------------------------------------------------------
technique Olas
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_main();
        PixelShader = compile ps_3_0 ps_main();
    }
}