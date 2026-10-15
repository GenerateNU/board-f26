using System.Collections.Generic; // List<T> instead of System.Collections.Generic.List<T>
using UnityEngine;

public abstract class BasicNode : MonoBehaviour
{
    [Tooltip("Fields which change per node.")]
    [SerializeField] private List<BasicNode> neighbors = new();
    public IReadOnlyList<BasicNode> Neighbors => neighbors;
    [Tooltip("Should be consistent across most objects of this prefab.")]
    [SerializeField] private GameObject lilypadGlow;
    public bool IsOccupied { get { return MaxFrogsOnPad == CurrentFrogsOnPad; } }
    public abstract bool IsTraversable { get; }
    public abstract NodeHazard NodeType { get; }
    public virtual int MaxFrogsOnPad { get { return 1; } }
    public virtual int CurrentFrogsOnPad { get; set; }

    protected virtual void Start()
    {
        CurrentFrogsOnPad = 0;
    }

    // if node is walkable by current frog, highlight/vfx/etc. glows only for this node
    // Can make overrides for this if we want different vfx for different node types
    public void Highlight(bool enable)
    {
        lilypadGlow.SetActive(enable);
    }

    // Called by FrogMove: true at the start of its turn, false when it hops or its turn ends.
    public void HighlightNeighbors(bool enable)
    {
        for (int i = 0; i < Neighbors.Count; i++)
        {
            if (neighbors[i] == null) { continue; }

            // Only light up pads the frog can actually move to,
            // but always turn them all off so nothing gets stuck glowing.
            if (!enable || Neighbors[i].IsTraversable)
            {
                Neighbors[i].Highlight(enable);
            }
        }
    }

    public virtual void CroakUsedOnPad() { }

    public void SpawnObjectOnPad(GameObject obj)
    {
        GameObject worldObj = Instantiate(obj, transform.position, Quaternion.identity);
        worldObj.transform.SetParent(gameObject.transform, true);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (BasicNode node in Neighbors)
        {
            // to help with in scene editing not throwing errors at you upon deleting 
            if (node == null) { return; }
            Gizmos.DrawLine(gameObject.transform.position, node.transform.position);
        }
    }
}
