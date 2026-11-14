#ifndef HIT_POINTS_INCLUDED
#define HIT_POINTS_INCLUDED

// Must match MaxHits in HitPointsToShader.cs
#define MAX_HITS 56

// Set from C#. Do NOT add these to the Shader Graph Blackboard.
float4 _HitPoints[MAX_HITS]; // xyz = world position, w = time of hit
float _HitCount;

// Custom Function node: Type = File, Name = HitGlow
void HitGlow_float(float3 WorldPos, float Radius, float FadeTime, float Time, out float Out)
{
    Out = 0;
    int count = min((int)_HitCount, MAX_HITS); // never read past the array, even if C# sends more

    for (int i = 0; i < count; i++)
    {
        float age = Time - _HitPoints[i].w;
        if (age < 0 || age > FadeTime) continue;
        float d = distance(WorldPos.xz, _HitPoints[i].xz);
        //float d = distance(WorldPos, _HitPoints[i].xyz);
        float glow = 1 - smoothstep(0, Radius, d);  // 1 at the hit, 0 at Radius
        float fade = 1 - saturate(age / FadeTime);  // fades out over FadeTime

        Out = max(Out, glow * fade);
    }
}

#endif
