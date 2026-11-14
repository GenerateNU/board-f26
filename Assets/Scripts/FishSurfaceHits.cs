using System.Collections.Generic;
using UnityEngine;

// Registers a water hit whenever a fish particle crosses the *visible* water surface.
// Put this on the FishSchools root (it scans every ParticleSystem underneath) or on a single school.
//
// The surface is rebuilt on the CPU each frame by mirroring the vertex stage of Water.shadergraph:
//   positionOS + normalOS * GradientNoise(uv0 + (_Speed * t, _Speed * t), Scale)
// (Gradient Noise node, Hash Type = Deterministic). The heights are then interpolated across the
// mesh triangles the same way the GPU rasterises them, so a fish is tested against what you see.
// If you change the vertex part of the graph, update SampleVertexOffset / noiseScale to match.
public class FishSurfaceHits : MonoBehaviour
{
    [Tooltip("Water renderer that receives the hits.")]
    public HitPointsToShader water;

    [Tooltip("Must match the Scale input on the Gradient Noise node in Water.shadergraph.")]
    public float noiseScale = 4.95f;

    [Tooltip("Material float used as the wave scroll speed (the graph's Speed property).")]
    public string speedProperty = "_Speed";

    [Tooltip("Register fish breaking the surface on the way up.")]
    public bool hitOnRise = true;

    [Tooltip("Register fish dropping back below the surface.")]
    public bool hitOnDive = true;

    [Tooltip("Minimum seconds between two hits from the same fish.")]
    public float perFishCooldown = 0.3f;

    public bool logHits = false;

    ParticleSystem[] systems;
    ParticleSystem.Particle[] buffer = new ParticleSystem.Particle[64];

    // Water mesh data (object space) and the displaced surface (world space), rebuilt per frame.
    Vector3[] meshVerts, meshNormals;
    Vector2[] meshUVs;
    int[] meshTris;
    Vector3[] surface;
    Renderer waterRenderer;

    // Spatial grid over the mesh's local X/Z: each cell lists the triangles that overlap it,
    // so a lookup tests ~2 triangles instead of all of them.
    const int GridCells = 16;
    int[][] grid;
    float gridMinX, gridMinZ, gridCellX, gridCellZ;

    struct FishState { public bool above; public float lastHit; public int frame; }

    // Particles have no stable index, but each one keeps its randomSeed for its whole life.
    readonly Dictionary<uint, FishState> states = new Dictionary<uint, FishState>();
    readonly List<uint> stale = new List<uint>();

    void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>();
        CacheWaterMesh();
    }

    void CacheWaterMesh()
    {
        if (water == null) return;
        waterRenderer = water.GetComponent<Renderer>();
        var mf = water.GetComponent<MeshFilter>();
        var mesh = mf != null ? mf.sharedMesh : null;
        if (mesh == null || !mesh.isReadable)
        {
            Debug.LogError("FishSurfaceHits: water mesh is missing or not Read/Write enabled.", this);
            water = null;
            return;
        }
        meshVerts = mesh.vertices;
        meshNormals = mesh.normals;
        meshUVs = mesh.uv;
        meshTris = mesh.triangles;
        surface = new Vector3[meshVerts.Length];
        BuildGrid();
    }

    void BuildGrid()
    {
        float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;
        foreach (var v in meshVerts)
        {
            minX = Mathf.Min(minX, v.x); maxX = Mathf.Max(maxX, v.x);
            minZ = Mathf.Min(minZ, v.z); maxZ = Mathf.Max(maxZ, v.z);
        }
        gridMinX = minX; gridMinZ = minZ;
        gridCellX = Mathf.Max((maxX - minX) / GridCells, 1e-6f);
        gridCellZ = Mathf.Max((maxZ - minZ) / GridCells, 1e-6f);

        var cells = new List<int>[GridCells * GridCells];
        for (int i = 0; i < cells.Length; i++) cells[i] = new List<int>();
        for (int t = 0; t < meshTris.Length; t += 3)
        {
            Vector3 a = meshVerts[meshTris[t]], b = meshVerts[meshTris[t + 1]], c = meshVerts[meshTris[t + 2]];
            int x0 = CellX(Mathf.Min(a.x, Mathf.Min(b.x, c.x))), x1 = CellX(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            int z0 = CellZ(Mathf.Min(a.z, Mathf.Min(b.z, c.z))), z1 = CellZ(Mathf.Max(a.z, Mathf.Max(b.z, c.z)));
            for (int cz = z0; cz <= z1; cz++)
                for (int cx = x0; cx <= x1; cx++)
                    cells[cz * GridCells + cx].Add(t);
        }
        grid = new int[cells.Length][];
        for (int i = 0; i < cells.Length; i++) grid[i] = cells[i].ToArray();
    }

    int CellX(float x) => Mathf.Clamp(Mathf.FloorToInt((x - gridMinX) / gridCellX), 0, GridCells - 1);
    int CellZ(float z) => Mathf.Clamp(Mathf.FloorToInt((z - gridMinZ) / gridCellZ), 0, GridCells - 1);

    void LateUpdate()
    {
        if (water == null) return;

        // Time node == _Time.y == time since level load.
        RebuildSurface(Time.timeSinceLevelLoad);

        int frame = Time.frameCount;

        foreach (var ps in systems)
        {
            int count = ps.particleCount;
            if (count == 0) continue;
            if (buffer.Length < count) buffer = new ParticleSystem.Particle[count];
            count = ps.GetParticles(buffer);

            bool local = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
            Transform t = ps.transform;

            for (int i = 0; i < count; i++)
            {
                uint id = buffer[i].randomSeed;
                Vector3 pos = local ? t.TransformPoint(buffer[i].position) : buffer[i].position;

                // Not over the water mesh: forget the fish so leaving/entering the edge isn't a "crossing".
                if (!TrySampleSurface(pos.x, pos.z, out float surfaceY))
                {
                    states.Remove(id);
                    continue;
                }

                bool above = pos.y > surfaceY;

                if (states.TryGetValue(id, out var s))
                {
                    bool crossed = s.above != above && ((above && hitOnRise) || (!above && hitOnDive));
                    if (crossed && Time.time - s.lastHit >= perFishCooldown)
                    {
                        water.AddHit(new Vector3(pos.x, surfaceY, pos.z));
                        s.lastHit = Time.time;
                        //if (logHits) Debug.Log($"Fish {(above ? "rose through" : "dove under")} the water at {pos}", this);
                    }
                }
                else
                {
                    s.lastHit = float.NegativeInfinity; // first sighting: just record which side it's on
                }

                s.above = above;
                s.frame = frame;
                states[id] = s;
            }
        }

        // Forget particles that died this frame.
        if (states.Count > 0)
        {
            stale.Clear();
            foreach (var kv in states) if (kv.Value.frame != frame) stale.Add(kv.Key);
            foreach (var id in stale) states.Remove(id);
        }
    }

    // ---- Surface reconstruction -------------------------------------------------------------

    // Displaces every water vertex exactly like the graph's vertex stage, then moves it to world space.
    public void RebuildSurface(float time)
    {
        if (water == null || meshVerts == null) return;
        float speed = waterRenderer.sharedMaterial.GetFloat(speedProperty);
        Vector2 offset = new Vector2(speed * time, speed * time); // Combine(R = Speed*Time, G = Speed*Time)
        Matrix4x4 l2w = water.transform.localToWorldMatrix;

        for (int v = 0; v < meshVerts.Length; v++)
        {
            float n = GradientNoise(meshUVs[v] + offset, noiseScale);
            surface[v] = l2w.MultiplyPoint3x4(meshVerts[v] + meshNormals[v] * n);
        }
    }

    // Height of the rendered (triangle-interpolated) surface at world x/z. False if outside the mesh.
    // The triangle is found in the water's local X/Z (undisplaced; the waves only move vertices along
    // their normals, which are up for a plane), then the displaced world positions are interpolated.
    public bool TrySampleSurface(float x, float z, out float y)
    {
        y = 0f;
        if (surface == null || grid == null) return false;

        // Project the world point onto the water's local X/Z.
        Vector3 lp = water.transform.InverseTransformPoint(new Vector3(x, water.transform.position.y, z));
        float lx = lp.x, lz = lp.z;
        int cx = Mathf.FloorToInt((lx - gridMinX) / gridCellX), cz = Mathf.FloorToInt((lz - gridMinZ) / gridCellZ);
        // Allow points exactly on the far edge; anything else outside the grid is off the water.
        if (cx == GridCells) cx--; if (cz == GridCells) cz--;
        if (cx < 0 || cz < 0 || cx >= GridCells || cz >= GridCells) return false;

        foreach (int i in grid[cz * GridCells + cx])
        {
            Vector3 a = meshVerts[meshTris[i]], b = meshVerts[meshTris[i + 1]], c = meshVerts[meshTris[i + 2]];
            float d = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
            if (Mathf.Abs(d) < 1e-12f) continue;
            float wa = ((b.z - c.z) * (lx - c.x) + (c.x - b.x) * (lz - c.z)) / d;
            float wb = ((c.z - a.z) * (lx - c.x) + (a.x - c.x) * (lz - c.z)) / d;
            float wc = 1f - wa - wb;
            const float eps = -1e-5f;
            if (wa < eps || wb < eps || wc < eps) continue;
            y = wa * surface[meshTris[i]].y + wb * surface[meshTris[i + 1]].y + wc * surface[meshTris[i + 2]].y;
            return true;
        }
        return false;
    }

    // ---- Shader Graph Gradient Noise (Hash Type = Deterministic), ported 1:1 --------------------
    // Source: com.unity.shadergraph GradientNoiseNode.cs + render-pipelines.core Hashes.hlsl.

    public static float GradientNoise(Vector2 uv, float scale)
    {
        float px = uv.x * scale, py = uv.y * scale;
        float ipx = Mathf.Floor(px), ipy = Mathf.Floor(py);
        float fpx = px - ipx, fpy = py - ipy;

        float d00 = Dot(Dir(ipx, ipy), fpx, fpy);
        float d01 = Dot(Dir(ipx, ipy + 1f), fpx, fpy - 1f);
        float d10 = Dot(Dir(ipx + 1f, ipy), fpx - 1f, fpy);
        float d11 = Dot(Dir(ipx + 1f, ipy + 1f), fpx - 1f, fpy - 1f);

        fpx = fpx * fpx * fpx * (fpx * (fpx * 6f - 15f) + 10f);
        fpy = fpy * fpy * fpy * (fpy * (fpy * 6f - 15f) + 10f);

        float lo = Mathf.LerpUnclamped(d00, d01, fpy);
        float hi = Mathf.LerpUnclamped(d10, d11, fpy);
        return Mathf.LerpUnclamped(lo, hi, fpx) + 0.5f;
    }

    static float Dot(Vector2 g, float x, float y) => g.x * x + g.y * y;

    static Vector2 Dir(float px, float py)
    {
        float x = HashTchou(px, py);
        return new Vector2(x - Mathf.Floor(x + 0.5f), Mathf.Abs(x) - 0.5f).normalized;
    }

    // Hash_Tchou_2_1_float
    static float HashTchou(float ix, float iy)
    {
        unchecked
        {
            uint vx = (uint)(int)System.Math.Round(ix, System.MidpointRounding.AwayFromZero);
            uint vy = (uint)(int)System.Math.Round(iy, System.MidpointRounding.AwayFromZero);
            vy ^= 1103515245u;
            vx += vy;
            vx *= vy;
            vx ^= vx >> 5;
            vx *= 0x27d4eb2du;
            return (vx >> 8) * (1.0f / 0x00ffffff);
        }
    }
}
