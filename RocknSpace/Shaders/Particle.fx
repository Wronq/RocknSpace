// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
float4 Overlay : OverlayColor;

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
	
	float2 pos = input.pos / 500;
	output.pos = float3(pos[0], pos[1], 1);
	output.vel = input.vel;
	output.col = input.col;
	
	return output;
}

[maxvertexcount(4)]
void GS( point GS_IN input[1], inout TriangleStream<PS_IN> triStream )
{
    PS_IN v = (PS_IN)0;
	v.col = input[0].col;

	float width = 0.002f;
	float length = input[0].vel[0] / 4;
	float alfa = input[0].vel[1];

	float3x3 translation = {1, 0, input[0].pos[0],
							0, 1, input[0].pos[1],
							0, 0, 1};

	float3x3 rotation = {cos(alfa), sin(alfa), 0,
						 -sin(alfa), cos(alfa), 0,
						 0, 0, 1};
	//bottom left
	float3 pos = float3(0, -width, 1);
	pos = mul(rotation, pos);
	pos = mul(translation, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//top left
	//v.pos = float4(input[0].pos[0], input[0].pos[1], 0, 1);
	pos = float3(0, width, 1);
	pos = mul(rotation, pos);
	pos = mul(translation, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//bottom right
	//v.pos = float4(input[0].pos[0] + input[0].vel[0], input[0].pos[1] + input[0].vel[1] - 0.1f, 0, 1);
	pos = float3(length, -width, 1);
	pos = mul(rotation, pos);
	pos = mul(translation, pos);

	v.pos = float4(pos[0], pos[1], 0, 1);
    triStream.Append(v);

	//top right
	//v.pos = float4(input[0].pos[0] + input[0].vel[0], input[0].pos[1] + input[0].vel[1], 0, 1);
	pos = float3(length, width, 1);
	pos = mul(rotation, pos);
	pos = mul(translation, pos);

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