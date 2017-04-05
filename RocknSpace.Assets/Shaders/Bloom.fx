// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.
#define SAMPLE_COUNT 15

struct VS_IN
{
    float4 pos : POSITION;
    float2 txcoord : TEXCOORD0;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 txcoord : TEXCOORD0;
	float4 color : COLOR;
};

Texture2D Texture1;
Texture2D BloomTexture;
Texture2D BaseTexture;

SamplerState colorSampler { AddressU = Clamp; AddressV = Clamp; };

float BloomThreshold;

float BloomIntensity;
float BaseIntensity;

float BloomSaturation;
float BaseSaturation;


float4 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];


float4 BloomExtract(PS_IN input) : SV_TARGET
{
    // Look up the original image color.
    float4 c = Texture1.Sample(colorSampler, input.txcoord);

	//return input.color;
    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}
 

//---------------------------------------


float4 GaussianBlur(PS_IN input) : SV_TARGET
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < SAMPLE_COUNT; i++)
    {
        c += Texture1.Sample(colorSampler, input.txcoord + float2(SampleOffsets[i][0], SampleOffsets[i][1])) * SampleWeights[i];
    }
    
	//return c * SampleWeights[0][0];
    return c;
}

//-----------------------------------------

// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}

float4 BloomCombine(PS_IN input) : SV_TARGET
{
    // Look up the bloom and original base image colors.
    float4 bloom = BloomTexture.Sample(colorSampler, input.txcoord);
    float4 base = BaseTexture.Sample(colorSampler,  input.txcoord);
    
    // Adjust color saturation and intensity.
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    base *= (1 - saturate(bloom));
    
    // Combine the two images.
    return base + bloom;
}

//----------------------------


PS_IN simpleVS( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	output.pos = input.pos;
	output.txcoord = input.txcoord;
	output.color = float4(input.pos[0] / 2 + 0.5, input.pos[1] / 2 + 0.5, 0, 1);

	return output;
}

technique10 Render
{
	pass p0
	{
		SetVertexShader( CompileShader( vs_4_0, simpleVS() ) );
		SetPixelShader( CompileShader( ps_4_0, BloomExtract() ) );
	}
	pass p1
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, simpleVS() ) );
		SetPixelShader( CompileShader( ps_4_0, GaussianBlur() ) );
	}
	pass p2
	{
		SetGeometryShader( 0 );
		SetVertexShader( CompileShader( vs_4_0, simpleVS() ) );
		SetPixelShader( CompileShader( ps_4_0, BloomCombine() ) );
	}
}