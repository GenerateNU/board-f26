using UnityEngine;

// The alligator AI (angry after turn then hit nodes within x neighbors) may be simple enough
// to put in this script

// Set up a basic skeleton for AI as well with alligatorAngry and DoAlligatorTurn

// Notes for future AI:
// we may need AlligatorTurn to return a list of Nodes hit and then check if players are 
// on any of them based on the player's lilypad ID (doesn't feel good) 
// or we may need to have nodes have access to player that's on them
public class AlligatorNode : BasicNode
{
    public override bool IsOccupied { get => base.IsOccupied; set { base.IsOccupied = value; alligatorAngry = true; } }
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.Alligator;
    private bool alligatorAngry = false;



    public void DoAlligatorTurn()
    {
        if (!alligatorAngry) { return; }
    }
}
