float4 Color;
float3 Position;
float2 Projection;
float2 Camera;

struct VS_IN
{
	float2 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

PS_IN VS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	float3 pos3 = float3(input.pos[0], input.pos[1], 1);

	float3x3 projection = { 1 / Projection[0], 0, Camera[0] / Projection[0],
						   0, 1 / Projection[1], Camera[1] / Projection[1],
						   0, 0, 1 };

	float3x3 translation = {1, 0, Position[0],
							0, 1, Position[1],
							0, 0, 1};

	float3x3 rotation = {cos(Position[2]), -sin(Position[2]), 0,
						 sin(Position[2]), cos(Position[2]), 0,
						 0, 0, 1};

	float3x3 ModelViewProjection = mul(projection, mul(translation, rotation));
	pos3 = mul(ModelViewProjection, pos3);

	output.pos = float4(pos3[0], pos3[1], 0, 1);
	output.col = Color;
	
	return output;
}

float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}

technique10 Render
{
	pass P0
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}