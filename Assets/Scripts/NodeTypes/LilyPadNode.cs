using System.Collections.Generic;
using UnityEngine;

public enum NodeHazard { None, Mites, Alligator, Winning }

public class LilyPadNode : BasicNode
{
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.None;
}
