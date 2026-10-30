using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Goes on the Egg prefab. An egg is a decoy: while any eggs exist, the fish chases
/// the nearest egg instead of frogs, and eats it when it reaches its pad.
/// </summary>
public class DecoyEgg : MonoBehaviour
{
    // All eggs currently in the scene (added on enable, removed when eaten/disabled).
    private static readonly List<DecoyEgg> _active = new List<DecoyEgg>();
    public static IReadOnlyList<DecoyEgg> ActiveDecoys => _active;

    [Tooltip("The pad this egg sits on. Set automatically when a frog lays it.")]
    [SerializeField] private BasicNode node;

    private bool _eaten;

    public BasicNode Node => node;
    public FrogActions Owner { get; private set; }

    // Clears the list when entering Play mode (in case domain reload is turned off).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _active.Clear();

    /// Called by FrogActions right after spawning the egg.
    public void Init(BasicNode pad, FrogActions owner)
    {
        node = pad;
        Owner = owner;
    }

    private void OnEnable()
    {
        if (!_eaten && !_active.Contains(this)) _active.Add(this);
    }

    private void OnDisable()
    {
        _active.Remove(this);
    }

    private void Start()
    {
        // Eggs placed by hand in a scene (not laid by a frog) find their pad themselves.
        if (node == null) node = FindNode();
        if (node == null) Debug.LogWarning($"{name}: couldn't find which pad it's on, so the fish can't target it.", this);
    }

    ///The fish calls this when it reaches the egg's pad.
    public void Eat()
    {
        if (_eaten) return;
        _eaten = true;
        _active.Remove(this); // remove now: Destroy() only happens at the end of the frame
        Debug.Log($"Fish ate the egg on {(node != null ? node.name : "?")}");
        Destroy(gameObject);
    }

    ///All uneaten eggs, nearest to the given position first.
    public static List<DecoyEgg> GetNearestFirst(Vector3 from)
    {
        var eggs = new List<DecoyEgg>();
        foreach (DecoyEgg egg in _active)
        {
            if (egg != null && !egg._eaten && egg.node != null) eggs.Add(egg);
        }
        eggs.Sort((a, b) =>
            Vector3.Distance(from, a.transform.position).CompareTo(Vector3.Distance(from, b.transform.position)));
        return eggs;
    }

    private BasicNode FindNode()
    {
        BasicNode parentNode = GetComponentInParent<BasicNode>();
        if (parentNode != null) return parentNode;

        // Fall back to the closest pad.
        BasicNode closest = null;
        float minDist = float.MaxValue;
        foreach (BasicNode candidate in FindObjectsByType<BasicNode>(FindObjectsSortMode.None))
        {
            float dist = Vector3.Distance(candidate.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = candidate;
            }
        }
        return closest;
    }
}