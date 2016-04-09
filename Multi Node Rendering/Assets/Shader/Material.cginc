//--------------------------------------------------------------------------------------
// CONSTANT Material Variables
//--------------------------------------------------------------------------------------

float vMaterialAmbient		= 0.25f;  //Ka := surface material's ambient coefficient
float4 vMaterialDiffuse		= 0.5f;   //Kd := surface material's diffuse coefficient
float4 vMaterialEmissive	= 0.0f;   //Ke := surface material's emissive coefficient
float4 vMaterialSpecular	= 0.0f;   //Ks := surface material's specular coefficient
float4 vMaterialReflect 	= 0.0f;   //Kr := surface material's reflectivity coefficient
float  sMaterialShininess	= 1.0f;	  //Ps := surface material's shininess
	

bool   bHasDiffuseMap		= false;
bool   bHasSpecularMap		= false;
bool   bHasNormalMap		= false;

bool   bHasDisplacementMap  = false;	
bool   bHasCubeMap			= false;
bool   bHasInstances		= false;
bool   bHasShadowMap		= false;

//--------------------------------------------------------------------------------------
// GLOBAL Variables
//--------------------------------------------------------------------------------------
float4 vLightAmbient		= float4(0.2f, 0.2f, 0.2f, 1.0f);


//--------------------------------------------------------------------------------------
// TEXTURES
//--------------------------------------------------------------------------------------
sampler2D	texDiffuseMap;
sampler2D	texSpecularMap;
sampler2D	texNormalMap;
sampler2D	texDisplacementMap;
//samplerCube texCubeMap;