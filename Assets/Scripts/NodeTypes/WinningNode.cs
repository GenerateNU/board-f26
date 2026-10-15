using UnityEngine;

public class WinningNode : LilyPadNode
{
    public override bool IsTraversable => true;
    public override NodeHazard NodeType => NodeHazard.Winning;
}
