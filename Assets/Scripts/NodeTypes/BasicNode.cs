using System.Collections.Generic; // List<T> instead of System.Collections.Generic.List<T>
using UnityEngine;

public abstract class BasicNode : MonoBehaviour
{
    // nodeID and future level design note: Technically this can be generated at the start of runtime 
    // as long as it isn't the same as any other node.
    // If we end up having really big levels we should have a nodeID creation script.
    [Tooltip("Fields which change per node.")]
    [SerializeField] private int nodeID;
    public int NodeID { get { return nodeID; } }
    [SerializeField] private List<BasicNode> neighbors = new();
    public IReadOnlyList<BasicNode> Neighbors => neighbors;
    [Tooltip("Should be consistent across most objects of this prefab.")]
    [SerializeField] private GameObject lilypadGlow;
    public virtual bool IsOccupied { get; set; }
    // Design note: At the moment Nodes aren't traverserable if a frog is already on one (I can't test this until turn system is up)
    // also wondering if we actually want it work this way based on the playtest, and if so how will we deal with it artwise
    public abstract bool IsTraversable { get; }
    public abstract NodeHazard NodeType { get; }


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
                Neighbors[i].Highlight(true);
            }
        }
    }

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
