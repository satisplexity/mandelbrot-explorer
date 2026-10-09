sampler2D input : register(s0);

float2 CenterHi : register(c0);
float2 CenterLo : register(c1);

float ScaleHi : register(c2);
float ScaleLo : register(c3);

float Aspect : register(c4);

float4 BackgroundColor : register(c5);
float4 EscapeColor     : register(c6);
float4 SetColor        : register(c7);

float2 ddQuickTwoSum(float a, float b)
{
    float s = a + b;
    float e = b - (s - a);

    return float2(s, e);
}

float2 ddTwoSum(float a, float b)
{
    float s = a + b;

    float v = s - a;

    float e = (a - (s - v)) + (b - v);

    return float2(s, e);
}

float2 ddNormalize(float2 a)
{
    return ddQuickTwoSum(a.x, a.y);
}

float2 ddAdd(float2 a, float2 b)
{
    float2 s = ddTwoSum(a.x, b.x);

    float e = s.y + a.y + b.y;

    float2 r = ddQuickTwoSum(s.x, e);

    return r;
}

float2 ddSub(float2 a, float2 b)
{
    float2 nb = float2(-b.x, -b.y);

    return ddAdd(a, nb);
}

float2 ddMul(float2 a, float2 b)
{
    const float SPLIT = 4097.0;

    float ah = a.x;
    float al = a.y;

    float bh = b.x;
    float bl = b.y;

    float ca = SPLIT * ah;

    float ahHi = ca - (ca - ah);
    float ahLo = ah - ahHi;

    float cb = SPLIT * bh;

    float bhHi = cb - (cb - bh);
    float bhLo = bh - bhHi;

    float p = ah * bh;

    float e = ((ahHi * bhHi - p) + ahHi * bhLo + ahLo * bhHi) + ahLo * bhLo;

    e += ah * bl;
    e += al * bh;
    e += al * bl;

    float2 r = ddQuickTwoSum(p, e);

    return r;
}

float2 ddMulFloat(float2 a, float b)
{
    return ddMul(a, float2(b, 0.0));
}

float2 ddSqr(float2 a)
{
    return ddMul(a, a);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 ndc = uv * 2.0 - 1.0;

    ndc.y = -ndc.y;

    float2 centerX = float2(CenterHi.x, CenterLo.x);

    float2 centerY = float2(CenterHi.y, CenterLo.y);

    float2 scale = float2(ScaleHi, ScaleLo);

    float offsetX = ndc.x * Aspect;

    float offsetY = ndc.y;

    float2 cr = ddAdd(centerX, ddMulFloat(scale, offsetX));

    float2 ci = ddAdd(centerY, ddMulFloat(scale, offsetY));

    float2 zr = float2(0.0, 0.0);
    float2 zi = float2(0.0, 0.0);

    const int MaxIterations = 254;

    float escaped = 0.0;
    float escapeIteration = (float)MaxIterations;


    [loop]
    for (int i = 0; i < MaxIterations; i++)
    {
        if (escaped < 0.5)
        {
            float2 zr2 = ddSqr(zr);

            float2 zi2 = ddSqr(zi);

            float2 zrzi =ddMul(zr,zi);

            float2 nextR = ddAdd(ddSub(zr2,zi2),cr);

            float2 twoZRZI = ddAdd(zrzi, zrzi);

            float2 nextI = ddAdd(twoZRZI, ci);

            zr = nextR;
            zi = nextI;

            float radius2 =
                zr.x * zr.x +
                zi.x * zi.x;

            if (radius2 > 4.0)
            {
                escaped = 1.0;
                escapeIteration = (float)i;
            }
        }
    }

    if (escaped < 0.5)
    {
        return SetColor;
    }

    float radius2 =
        zr.x * zr.x +
        zi.x * zi.x;

    float magnitude = sqrt(max(radius2, 1.000001));


    float smoothIteration =
        escapeIteration +
        1.0 -
        log(log(magnitude)) / log(2.0);


    float t = saturate(smoothIteration / (float)MaxIterations);

    return lerp(BackgroundColor, EscapeColor, t);
}