#define MAX_HEIGHT 16
float4 Color;
float Width;
float Height;
float Step;
float2 Disturb[MAX_HEIGHT];

struct VS_IN
{
	float2 pos : POSITION0;
};

struct GS_IN
{
	float2 pos : POSITION0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

GS_IN VS( VS_IN input )
{
	GS_IN output = (GS_IN)0;
	
	output.pos = input.pos;
	
	return output;
}

[maxvertexcount(30)]
void GS( point GS_IN input[1], inout TriangleStream<PS_IN> triStream )
{
	PS_IN v = (PS_IN)0;
	v.col = float4(1, Step, Width, 1);
	
	float2 widthVec = float2(Width, 0);

	//v.pos = float4(input[0].pos[0], input[0].pos[1] - 0 * Step, 0, 1);
	//triStream.Append(v);
	//v.pos = float4(input[0].pos[0] + 1, input[0].pos[1], 0, 1);
	//triStream.Append(v);
	//v.pos = float4(input[0].pos[0], input[0].pos[1] - 1, 0, 1);
	//triStream.Append(v);

	[loop]
	for (uint i = 0; i < (uint)(Height / Step) + 2 && i < 14; i++)
	{
		v.pos = float4(input[0].pos[0] + Disturb[i][0], Height - 1 + Disturb[i][1] - i * Step, 0, 1);
		triStream.Append(v);
		v.pos = v.pos + float4(widthVec[0], widthVec[1], 0, 0);
		triStream.Append(v);

		widthVec = Disturb[i + 2] - Disturb[i] - float2(0, 2 * Step);
		widthVec = normalize(float2(-widthVec[1], widthVec[0])) * Width;

		/*
		v.pos = float4(0, 0 - i * Step, 0, 1);
		triStream.Append(v);

		v.pos = float4(0 + Width, 0 - i * Step, 0, 1);
		triStream.Append(v);*/
	}
    /*PS_IN v = (PS_IN)0;
	v.col = input[0].col;

	float width = 1;
	float length = input[0].vel[0] * 50;
	float alfa = input[0].vel[1];

	float3x3 projection = { 1 / Projection[0], 0, 0,
						    0, 1 / Projection[1], 0,
						    0, 0, 1 };

	float3x3 translation = {1, 0, input[0].pos[0],
							0, 1, input[0].pos[1],
							0, 0, 1};

	float3x3 camera = { 1, 0, Camera[0],
					    0, 1, Camera[1],
		                0, 0, 1 };

	translation = mul(translation, camera);

	float3x3 rotation = {cos(alfa), sin(alfa), 0,
						 -sin(alfa), cos(alfa), 0,
						 0, 0, 1};

	float3x3 ModelViewProjection = mul(projection, mul(translation, rotation));

	//top left
	float3 pos = float3(-length, width, 1);
	pos = mul(ModelViewProjection, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//top right
	pos = float3(length, width, 1);
	pos = mul(ModelViewProjection, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//bottom left
	pos = float3(-length, -width, 1);
	pos = mul(ModelViewProjection, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//bottom right
	pos = float3(length, -width, 1);
	pos = mul(ModelViewProjection, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);*/
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}

technique10 Render
{
	pass P0
	{
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}