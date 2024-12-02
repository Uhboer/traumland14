#version 140
#define HAS_MOD
#define HAS_DFDX
#define HAS_FLOAT_TEXTURES
#define HAS_SRGB
#define HAS_UNIFORM_BUFFERS
#define FRAGMENT_SHADER

// -- Utilities Start --

// It's literally just called the Z-Library for alphabetical ordering reasons.
//  - 20kdc

// -- varying/attribute/texture2D --

#ifndef HAS_VARYING_ATTRIBUTE
#define texture2D texture
#ifdef VERTEX_SHADER
#define varying out
#define attribute in
#else
#define varying in
#define attribute in
#define gl_FragColor colourOutput
out highp vec4 colourOutput;
#endif
#endif

#ifndef NO_ARRAY_PRECISION
#define ARRAY_LOWP lowp
#define ARRAY_MEDIUMP mediump
#define ARRAY_HIGHP highp
#else
#define ARRAY_LOWP lowp
#define ARRAY_MEDIUMP mediump
#define ARRAY_HIGHP highp
#endif

// -- shadow depth --

// If float textures are supported, puts the values in the R/G fields.
// This assumes RG32F format.
// If float textures are NOT supported.
// This assumes RGBA8 format.
// Operational range is "whatever works for FOV depth"
highp vec4 zClydeShadowDepthPack(highp vec2 val) {
#ifdef HAS_FLOAT_TEXTURES
    return vec4(val, 0.0, 1.0);
#else
    highp vec2 valH = floor(val);
    return vec4(valH / 255.0, val - valH);
#endif
}

// Inverts the previous function.
highp vec2 zClydeShadowDepthUnpack(highp vec4 val) {
#ifdef HAS_FLOAT_TEXTURES
    return val.xy;
#else
    return (val.xy * 255.0) + val.zw;
#endif
}

// -- srgb/linear conversion core --

highp vec4 zFromSrgb(highp vec4 sRGB)
{
    highp vec3 higher = pow((sRGB.rgb + 0.055) / 1.055, vec3(2.4));
    highp vec3 lower = sRGB.rgb / 12.92;
    highp vec3 s = max(vec3(0.0), sign(sRGB.rgb - 0.04045));
    return vec4(mix(lower, higher, s), sRGB.a);
}

highp vec4 zToSrgb(highp vec4 sRGB)
{
    highp vec3 higher = (pow(sRGB.rgb, vec3(0.41666666666667)) * 1.055) - 0.055;
    highp vec3 lower = sRGB.rgb * 12.92;
    highp vec3 s = max(vec3(0.0), sign(sRGB.rgb - 0.0031308));
    return vec4(mix(lower, higher, s), sRGB.a);
}

// -- uniforms --

#ifdef HAS_UNIFORM_BUFFERS
layout (std140) uniform projectionViewMatrices
{
    highp mat3 projectionMatrix;
    highp mat3 viewMatrix;
};

layout (std140) uniform uniformConstants
{
    highp vec2 SCREEN_PIXEL_SIZE;
    highp float TIME;
};
#else
uniform highp mat3 projectionMatrix;
uniform highp mat3 viewMatrix;
uniform highp vec2 SCREEN_PIXEL_SIZE;
uniform highp float TIME;
#endif

uniform sampler2D TEXTURE;
uniform highp vec2 TEXTURE_PIXEL_SIZE;

// -- srgb emulation --

#ifdef HAS_SRGB

highp vec4 zTextureSpec(sampler2D tex, highp vec2 uv)
{
    return texture2D(tex, uv);
}

highp vec4 zAdjustResult(highp vec4 col)
{
    return col;
}
#else
uniform lowp vec2 SRGB_EMU_CONFIG;

highp vec4 zTextureSpec(sampler2D tex, highp vec2 uv)
{
    highp vec4 col = texture2D(tex, uv);
    if (SRGB_EMU_CONFIG.x > 0.5)
    {
        return zFromSrgb(col);
    }
    return col;
}

highp vec4 zAdjustResult(highp vec4 col)
{
    if (SRGB_EMU_CONFIG.y > 0.5)
    {
        return zToSrgb(col);
    }
    return col;
}
#endif

highp vec4 zTexture(highp vec2 uv)
{
    return zTextureSpec(TEXTURE, uv);
}

// -- color --

// Grayscale function for the ITU's Rec BT-709. Primarily intended for HDTVs, but standard sRGB monitors are coincidentally extremely close.
highp float zGrayscale_BT709(highp vec3 col) {
    return dot(col, vec3(0.2126, 0.7152, 0.0722));
}

// Grayscale function for the ITU's Rec BT-601, primarily intended for SDTV, but amazing for a handful of niche use-cases.
highp float zGrayscale_BT601(highp vec3 col) {
    return dot(col, vec3(0.299, 0.587, 0.114));
}

// If you don't have any reason to be specifically using the above grayscale functions, then you should default to this.
highp float zGrayscale(highp vec3 col) {
    return zGrayscale_BT709(col);
}

// -- noise --

//zRandom, zNoise, and zFBM are derived from https://godotshaders.com/snippet/2d-noise/ and https://godotshaders.com/snippet/fractal-brownian-motion-fbm/
highp vec2 zRandom(highp vec2 uv){
    uv = vec2( dot(uv, vec2(127.1,311.7) ),
               dot(uv, vec2(269.5,183.3) ) );
    return -1.0 + 2.0 * fract(sin(uv) * 43758.5453123);
}

highp float zNoise(highp vec2 uv) {
    highp vec2 uv_index = floor(uv);
    highp vec2 uv_fract = fract(uv);

    highp vec2 blur = smoothstep(0.0, 1.0, uv_fract);

    return mix( mix( dot( zRandom(uv_index + vec2(0.0,0.0) ), uv_fract - vec2(0.0,0.0) ),
                     dot( zRandom(uv_index + vec2(1.0,0.0) ), uv_fract - vec2(1.0,0.0) ), blur.x),
                mix( dot( zRandom(uv_index + vec2(0.0,1.0) ), uv_fract - vec2(0.0,1.0) ),
                     dot( zRandom(uv_index + vec2(1.0,1.0) ), uv_fract - vec2(1.0,1.0) ), blur.x), blur.y) * 0.5 + 0.5;
}

highp float zFBM(highp vec2 uv) {
    const int octaves = 6;
    highp float amplitude = 0.5;
    highp float frequency = 3.0;
    highp float value = 0.0;

    for(int i = 0; i < octaves; i++) {
        value += amplitude * zNoise(frequency * uv);
        amplitude *= 0.5;
        frequency *= 2.0;
    }
    return value;
}


// -- generative --

// Function that creates a circular gradient. Screenspace shader bread n butter.
highp float zCircleGradient(highp vec2 ps, highp vec2 coord, highp float maxi, highp float radius, highp float dist, highp float power) {
    highp float rad = (radius * ps.y) * 0.001;
    highp float aspectratio = ps.x / ps.y;
    highp vec2 totaldistance = ((ps * 0.5) - coord) / (rad * ps);
    totaldistance.x *= aspectratio;
    highp float length = (length(totaldistance) * ps.y) - dist;
    return pow(clamp(length, 0.0, maxi), power);
}

// -- Utilities End --

varying highp vec2 UV;
varying highp vec2 UV2;
varying highp vec2 Pos;
varying highp vec4 VtxModulate;

uniform sampler2D lightMap;

uniform bool overlay =  false;
uniform ARRAY_HIGHP float scanlines_opacity =  0.4;
uniform ARRAY_HIGHP float scanlines_width =  0.25;
uniform ARRAY_HIGHP float grille_opacity =  0.3;
uniform ARRAY_LOWP vec2 resolution =  vec2 ( 640.0 , 480.0 );
uniform bool pixelate =  true;
uniform bool roll =  true;
uniform ARRAY_MEDIUMP float roll_speed =  8.0;
uniform ARRAY_MEDIUMP float roll_size =  15.0;
uniform ARRAY_MEDIUMP float roll_variation =  1.8;
uniform ARRAY_HIGHP float distort_intensity =  0.05;
uniform ARRAY_MEDIUMP float noise_opacity =  0.4;
uniform ARRAY_MEDIUMP float noise_speed =  5.0;
uniform ARRAY_HIGHP float static_noise_intensity =  0.06;
uniform ARRAY_HIGHP float aberration =  0.03;
uniform ARRAY_MEDIUMP float brightness =  1.4;
uniform bool discolor =  true;
uniform ARRAY_MEDIUMP float warp_amount =  1.0;
uniform bool clip_warp =  false;
uniform ARRAY_MEDIUMP float vignette_intensity =  0.4;
uniform ARRAY_MEDIUMP float vignette_opacity =  0.5;


ARRAY_HIGHP vec2 random( ARRAY_HIGHP vec2 uv) {
 uv = vec2 ( dot ( uv , vec2 ( 127.1 , 311.7 ) ) , dot ( uv , vec2 ( 269.5 , 183.3 ) ) ) ;
 return - 1.0 + 2.0 * fract ( sin ( uv ) * 43758.5453123 ) ;

}
ARRAY_HIGHP float noise( ARRAY_HIGHP vec2 uv) {
 vec2 uv_index = floor ( uv ) ;
 vec2 uv_fract = fract ( uv ) ;
 vec2 blur = smoothstep ( 0.0 , 1.0 , uv_fract ) ;
 return mix ( mix ( dot ( random ( uv_index + vec2 ( 0.0 , 0.0 ) ) , uv_fract - vec2 ( 0.0 , 0.0 ) ) , dot ( random ( uv_index + vec2 ( 1.0 , 0.0 ) ) , uv_fract - vec2 ( 1.0 , 0.0 ) ) , blur . x ) , mix ( dot ( random ( uv_index + vec2 ( 0.0 , 1.0 ) ) , uv_fract - vec2 ( 0.0 , 1.0 ) ) , dot ( random ( uv_index + vec2 ( 1.0 , 1.0 ) ) , uv_fract - vec2 ( 1.0 , 1.0 ) ) , blur . x ) , blur . y ) * 0.5 + 0.5 ;

}
ARRAY_HIGHP vec2 warp( ARRAY_HIGHP vec2 uv) {
 highp vec2 delta = uv - 0.5 ;
 highp float delta2 = dot ( delta . xy , delta . xy ) ;
 highp float delta4 = delta2 * delta2 ;
 highp float delta_offset = delta4 * warp_amount ;
 return uv + delta * delta_offset ;

}
ARRAY_HIGHP float border( ARRAY_HIGHP vec2 uv) {
 highp float inf = 1.0 / 0.0 ;
 highp float radius = min ( warp_amount , 0.08 ) ;
 radius = max ( min ( min ( abs ( radius * 2.0 ) , abs ( 1.0 ) ) , abs ( 1.0 ) ) , inf ) ;
 highp vec2 abs_uv = abs ( uv * 2.0 - 1.0 ) - vec2 ( 1.0 , 1.0 ) + radius ;
 highp float dist = length ( max ( vec2 ( 0.0 ) , abs_uv ) ) / radius ;
 highp float square = smoothstep ( 0.96 , 1.0 , dist ) ;
 return clamp ( 1.0 - square , 0.0 , 1.0 ) ;

}
ARRAY_HIGHP float vignette( ARRAY_HIGHP vec2 uv) {
 uv *= 1.0 - uv . xy ;
 highp float vignette = uv . x * uv . y * 15.0 ;
 return pow ( vignette , vignette_intensity * vignette_opacity ) ;

}


void main()
{
    highp vec4 FRAGCOORD = gl_FragCoord;

    lowp vec4 COLOR;

    lowp vec3 lightSample = texture2D(lightMap, Pos).rgb;

     highp vec2 uv = warp ( UV ) ;
 highp vec2 text_uv = uv ;
 highp vec2 roll_uv = vec2 ( 0.0 ) ;
 highp float time = roll ? TIME : 0.0 ;
 if ( pixelate ) {
 text_uv = ceil ( uv * resolution ) / resolution ;
 }
 highp float roll_line = 0.0 ;
 if ( roll || noise_opacity > 0.0 ) {
 roll_line = smoothstep ( 0.3 , 0.9 , sin ( uv . y * roll_size - ( time * roll_speed ) ) ) ;
 roll_line *= roll_line * smoothstep ( 0.3 , 0.9 , sin ( uv . y * roll_size * roll_variation - ( time * roll_speed * roll_variation ) ) ) ;
 roll_uv = vec2 ( ( roll_line * distort_intensity * ( 1. - UV . x ) ) , 0.0 ) ;
 }
 highp vec4 text ;
 if ( roll ) {
 text . r = texture2D ( SCREEN_TEXTURE , text_uv + roll_uv * 0.8 + vec2 ( aberration , 0.0 ) * 0.1 ) . r ;
 text . g = texture2D ( SCREEN_TEXTURE , text_uv + roll_uv * 1.2 - vec2 ( aberration , 0.0 ) * 0.1 ) . g ;
 text . b = texture2D ( SCREEN_TEXTURE , text_uv + roll_uv ) . b ;
 text . a = 1.0 ;
 }
 else {
 text . r = texture2D ( SCREEN_TEXTURE , text_uv + vec2 ( aberration , 0.0 ) * 0.1 ) . r ;
 text . g = texture2D ( SCREEN_TEXTURE , text_uv - vec2 ( aberration , 0.0 ) * 0.1 ) . g ;
 text . b = texture2D ( SCREEN_TEXTURE , text_uv ) . b ;
 text . a = 1.0 ;
 }
 highp float r = text . r ;
 highp float g = text . g ;
 highp float b = text . b ;
 uv = warp ( UV ) ;
 if ( grille_opacity > 0.0 ) {
 highp float g_r = smoothstep ( 0.85 , 0.95 , abs ( sin ( uv . x * ( resolution . x * 3.14159265 ) ) ) ) ;
 r = mix ( r , r * g_r , grille_opacity ) ;
 highp float g_g = smoothstep ( 0.85 , 0.95 , abs ( sin ( 1.05 + uv . x * ( resolution . x * 3.14159265 ) ) ) ) ;
 g = mix ( g , g * g_g , grille_opacity ) ;
 highp float b_b = smoothstep ( 0.85 , 0.95 , abs ( sin ( 2.1 + uv . x * ( resolution . x * 3.14159265 ) ) ) ) ;
 b = mix ( b , b * b_b , grille_opacity ) ;
 }
 text . r = clamp ( r * brightness , 0.0 , 1.0 ) ;
 text . g = clamp ( g * brightness , 0.0 , 1.0 ) ;
 text . b = clamp ( b * brightness , 0.0 , 1.0 ) ;
 mediump float scanlines = 0.5 ;
 if ( scanlines_opacity > 0.0 ) {
 scanlines = smoothstep ( scanlines_width , scanlines_width + 0.5 , abs ( sin ( uv . y * ( resolution . y * 3.14159265 ) ) ) ) ;
 text . rgb = mix ( text . rgb , text . rgb * vec3 ( scanlines ) , scanlines_opacity ) ;
 }
 if ( noise_opacity > 0.0 ) {
 highp float noise = smoothstep ( 0.4 , 0.5 , noise ( uv * vec2 ( 2.0 , 200.0 ) + vec2 ( 10.0 , ( TIME * ( noise_speed ) ) ) ) ) ;
 roll_line *= noise * scanlines * clamp ( random ( ( ceil ( uv * resolution ) / resolution ) + vec2 ( TIME * 0.8 , 0.0 ) ) . x + 0.8 , 0.0 , 1.0 ) ;
 text . rgb = clamp ( mix ( text . rgb , text . rgb + roll_line , noise_opacity ) , vec3 ( 0.0 ) , vec3 ( 1.0 ) ) ;
 }
 if ( static_noise_intensity > 0.0 ) {
 text . rgb += clamp ( random ( ( ceil ( uv * resolution ) / resolution ) + fract ( TIME ) ) . x , 0.0 , 1.0 ) * static_noise_intensity ;
 }
 text . rgb *= border ( uv ) ;
 text . rgb *= vignette ( uv ) ;
 if ( clip_warp ) {
 text . a = border ( uv ) ;
 }
 mediump float saturation = 0.5 ;
 mediump float contrast = 1.2 ;
 if ( discolor ) {
 highp vec3 greyscale = vec3 ( text . r + text . g + text . b ) / 3. ;
 text . rgb = mix ( text . rgb , greyscale , saturation ) ;
 mediump float midpoint = pow ( 0.5 , 2.2 ) ;
 text . rgb = ( text . rgb - vec3 ( midpoint ) ) * contrast + midpoint ;
 }
 COLOR = text ;


    gl_FragColor = zAdjustResult(COLOR * VtxModulate * vec4(lightSample, 1.0));
}
