using System.Collections.Generic; // List<T> instead of System.Collections.Generic.List<T>
using UnityEngine;

// for other scripts sprint 2
public enum NodeHazard { None, Mites, Alligator }

public class LilyPadNode : MonoBehaviour
{
    [SerializeField] private int nodeID;
    public int NodeID { get { return nodeID; } }
    [SerializeField] private List<LilyPadNode> neighbors = new();
    [SerializeField] private NodeHazard hazard;
    public NodeHazard Hazard { get { return hazard; } }
    [SerializeField] private bool isGoalPad;
    [SerializeField] private Material noHazardMat;
    [SerializeField] private Material miteMat;
    [SerializeField] private Material alligatorMat;
    [SerializeField] private GameObject lilypadGlow;

    //needs to be public so frogs/fish can read it
    public IReadOnlyList<LilyPadNode> Neighbors => neighbors;

    public bool IsOccupied { get; set; }
    public bool IsTraversable => Hazard != NodeHazard.Mites && !IsOccupied;

    void Start()
    {
        SetHazard(this.hazard);
    }

    // if node is walkable by current frog, highlight/vfx/etc.
    public void Highlight(bool enable)
    {
        lilypadGlow.SetActive(enable);
    }

    public void SetHazard(NodeHazard newHazard)
    {
        this.hazard = newHazard;
        UpdateHazard(this.hazard);
    }

    private void UpdateHazard(NodeHazard newHazard)
    {
        Material newMaterial = newHazard switch
        {
            NodeHazard.None => noHazardMat,
            NodeHazard.Mites => miteMat,
            NodeHazard.Alligator => alligatorMat,
            _ => throw new System.Exception($"No material for {newHazard}")
        };
        Renderer renderer = this.gameObject.GetComponent<Renderer>();
        renderer.material = newMaterial;
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
