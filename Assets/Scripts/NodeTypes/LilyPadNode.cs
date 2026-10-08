using System.Collections.Generic;
using UnityEngine;

public enum NodeHazard { None, Mites, Alligator }

public class LilyPadNode : BasicNode
{
    // This may need to be in the BasicNode class, likely fine here though? 
    // Unsure how we'd get this information to GameController either way 
    [SerializeField] private bool isGoalPad;
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.None;
}
