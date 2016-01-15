float4 Overlay : OverlayColor;
float2 Projection;
float2 Camera;

struct VS_IN
{
	float2 pos : POSITION0;
	float2 vel : POSITION1;
	float4 col : COLOR0;
};

struct GS_IN
{
	float3 pos : POSITION0;
	float2 vel : POSITION1;
	float4 col : COLOR0;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

GS_IN VS( VS_IN input )
{
	GS_IN output = (GS_IN)0;
	
	output.pos = float3(input.pos[0], input.pos[1], 1);
	output.vel = input.vel;
	output.col = input.col;
	
	return output;
}

[maxvertexcount(4)]
void GS( point GS_IN input[1], inout TriangleStream<PS_IN> triStream )
{
    PS_IN v = (PS_IN)0;
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
    triStream.Append(v);
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