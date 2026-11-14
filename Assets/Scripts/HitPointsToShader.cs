using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HitPointsToShader : MonoBehaviour
{
    const int MaxHits = 56; // must match MAX_HITS in HitPoints.hlsl

    static readonly int HitPointsID = Shader.PropertyToID("_HitPoints");
    static readonly int HitCountID = Shader.PropertyToID("_HitCount");

    Material mat;
    readonly Vector4[] hits = new Vector4[MaxHits];
    int next;   // ring buffer: newest hit overwrites the oldest
    int count;

    void Awake()
    {
        mat = GetComponent<Renderer>().material;
        // The first array set locks its size, so always send the full MaxHits length.
        mat.SetVectorArray(HitPointsID, hits);
        mat.SetFloat(HitCountID, 0);
    }

    // Call this from anywhere: collisions, raycasts, particle collision events.
    public void AddHit(Vector3 worldPoint)
    {
        // timeSinceLevelLoad matches the shader's Time node (_Time.y); Time.time does not.
        hits[next] = new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, Time.timeSinceLevelLoad);
        next = (next + 1) % MaxHits;
        count = Mathf.Min(count + 1, MaxHits);

        mat.SetVectorArray(HitPointsID, hits);
        mat.SetFloat(HitCountID, count);
    }

    // void OnCollisionEnter(Collision collision)
    // {
    //     for (int i = 0; i < collision.contactCount; i++)
    //         AddHit(collision.GetContact(i).point);
    // }

    readonly System.Collections.Generic.List<ParticleCollisionEvent> particleEvents =
        new System.Collections.Generic.List<ParticleCollisionEvent>();

    // On the object being hit, 'other' is the particle system that hit it.
    void OnParticleCollision(GameObject other)
    {
        if (!other.TryGetComponent(out ParticleSystem ps)) return;

        int n = ps.GetCollisionEvents(gameObject, particleEvents);
        for (int i = 0; i < n; i++)
        {
            AddHit(particleEvents[i].intersection);
            Debug.Log("particle hit");
        }
    }
}
