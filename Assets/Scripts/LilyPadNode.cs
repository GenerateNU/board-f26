using System.Collections.Generic; // List<T> instead of System.Collections.Generic.List<T>
using UnityEngine;

public class LilyPadNode : MonoBehaviour
{
    [SerializeField] private List<LilyPadNode> neighbors = new();
    
    //needs to be public so frogs/fish can read it
    public IReadOnlyList<LilyPadNode> Neighbors => neighbors;

    public bool IsOccupied { get; set; }
}
