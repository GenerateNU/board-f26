using System.Collections.Generic; // List<T> instead of System.Collections.Generic.List<T>
using UnityEngine;

// for other scripts sprint 2
public enum NodeHazard { None, Mites, Alligator }

public class LilyPadNode : MonoBehaviour
{
    [SerializeField] private int nodeID;
    public int NodeID { get { return nodeID; } }
    [SerializeField] private List<LilyPadNode> neighbors = new();

    //needs to be public so frogs/fish can read it
    public IReadOnlyList<LilyPadNode> Neighbors => neighbors;

    public bool IsOccupied { get; set; }

    // skels for sprint 2
    public NodeHazard Hazard { get; set; } = NodeHazard.None;
    public bool IsTraversable => Hazard != NodeHazard.Mites && !IsOccupied;

    public void Highlight(bool enable) 
    { 
        // if node is walkable by current frog, highlight/vfx/etc.
    }

    public void SetHazard(NodeHazard hazard) 
    { 
        Hazard = hazard; 
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (LilyPadNode node in Neighbors)
        {
            Gizmos.DrawLine(gameObject.transform.position, node.transform.position);
        }
    }
}
