//---------------------------------------------
// Bloom


float4 main(float2 texCoord: TEXCOORD0,
		uniform sampler RT: register(s0),
		uniform sampler Blur1: register(s1),
		uniform float OriginalImageWeight,
		uniform float BlurWeight
		) : COLOR {


	float4 sharp = tex2D(RT,   texCoord);
	float4 blur  = tex2D(Blur1, texCoord);

	/*int samples = 24;
	float2 dir = texCoord - float2( 0.5f, 0.5f );
	float4 motion_blurred = 0;
	float len = length( dir );
	dir = normalize( dir );
	len = saturate( len - 0.25f ) * 5.0f;
	dir *= len;
	for ( float i = 0; i < samples; i++ )
	{
		motion_blurred += tex2D(RT, texCoord - i/samples * dir * 0.015f ) * ( i/samples );
	}
	motion_blurred /= motion_blurred.a;*/
	float4 motion_blurred = tex2D(RT, texCoord );

	sharp = motion_blurred;

	//return blur*0.5+sharp*0.5;
	return blur*BlurWeight+sharp*OriginalImageWeight;
	//return blur;

	//return ( sharp + blur * 1.8 ) / 2;

//	return (sharp*3/6 + (blur*4/6))*float4(1.5, 1.5, 1.5, 1);;

//	float4 color	= lerp( sharp, blur, 0.4f );

//	return color;


/*
	return ( sharp + blur * 1.8 ) / 2 +
			 luminance(blur) * 
			 float4( 0.5, 0.5, 0.5, 0) +
			 luminance(sharp) *
			 float4( 0.3, 0.3, 0.3, 0);
*/
/*
	return ( sharp + blur * 0.9) / 2 +
			 luminance(blur) * float4(0.1, 0.15, 0.7, 0);
*/

/*
	return ( sharp + blur * 0.9) / 2 +
			 luminance(blur) * float4(0.1, 0.15, 0.7, 0);
*/

//	float4 retColor = luminance( sharp ) +
//							luminance( blur ) + blur / 2;
//	return retColor;
}









