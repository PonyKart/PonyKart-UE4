void main_vp(float2 uv : TEXCOORD0,
			 float4 position : POSITION,
			 float4 color : COLOR0,
			 
			 out float2 oUv : TEXCOORD0,			 
			 out float4 oPosition : POSITION,
			 out float4 oColor : COLOR0,
			 
			 uniform int numTilesU,
			 uniform int numTilesV,
			 
			 uniform float4x4 worldViewProj)
{


numTilesU = 8;
numTilesV = 8;
int numTilesTotal = numTilesU*numTilesV;
int selectedTile = (int)(numTilesTotal * color.a);
if (selectedTile == numTilesTotal)
	selectedTile = numTilesTotal - 1;
oUv.x = (uv.x+selectedTile%numTilesU)/numTilesU;///selectedTile;
oUv.y = (uv.y+selectedTile/numTilesU)/numTilesV;///selectedTile;

	oPosition = mul(worldViewProj, position);
	oColor = color;
}

float4 main_fp (float2 uv : TEXCOORD0,
				uniform sampler2D tex : register(s0))
: COLOR
{
	return tex2D(tex, uv.xy);
	//return float4(1, 1, 1, 1);
}