#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix MatrixTransform;
Texture2D SpriteTexture;
sampler2D SpriteTextureSampler : register(s0) = sampler_state { Texture = <SpriteTexture>; };

float Saturation = 1.0;   // 0 = tons de cinza
float3 Tint = float3(1,1,1);

struct VSOutput { float4 position : SV_Position; float4 color : COLOR0; float2 texcoord : TEXCOORD0; };

VSOutput MainVS(float4 position : POSITION0, float4 color : COLOR0, float2 texcoord : TEXCOORD0)
{
    VSOutput o;
    o.position = mul(position, MatrixTransform);
    o.color = color;
    o.texcoord = texcoord;
    return o;
}

float4 MainPS(VSOutput input) : COLOR0
{
    float4 c = tex2D(SpriteTextureSampler, input.texcoord) * input.color;
    float g = dot(c.rgb, float3(0.299, 0.587, 0.114));
    c.rgb = lerp(float3(g, g, g), c.rgb, Saturation) * Tint;
    return c;
}

technique PostProcess { pass P0 { VertexShader = compile VS_SHADERMODEL MainVS(); PixelShader = compile PS_SHADERMODEL MainPS(); } }
