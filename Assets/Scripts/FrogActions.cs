using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum ActionType
{
    Croak,
    LayEgg,
    HopAgain,
    EndTurn
}

public class FrogActions : MonoBehaviour
{
    // sprint 2 skel
    // needs to be public to check for respawn
    public BasicNode respawnNode;

    [SerializeField]
    private GameObject eggPrefab;
    private FrogMove frogMove;

    public bool HasActedThisTurn { get; set; } = false;

    void Start()
    {
        frogMove = gameObject.GetComponent<FrogMove>();
    }

    public bool IsActionAvailable(ActionType action)
    {
        switch (action)
        {
            case ActionType.Croak:
                return !HasActedThisTurn;
            case ActionType.HopAgain:
                return !HasActedThisTurn;
            case ActionType.LayEgg:
                return !HasActedThisTurn && respawnNode == null;
            case ActionType.EndTurn:
                return true;
            default:
                return false;
        }
    }

    public void DoAction(ActionType action)
    {
        switch (action)
        {
            case ActionType.Croak:
                Croak();
                break;
            case ActionType.HopAgain:
                HopAgain();
                break;
            case ActionType.LayEgg:
                LayEgg();
                break;
            case ActionType.EndTurn:
                EndTurn();
                break;
            default:
                return;
        }
    }

    // croak, lay egg, hop again etc.
    private void Croak()
    {
        foreach (BasicNode node in frogMove.GetLilyPadFrogOn().Neighbors)
        {
            UseCroakOnNode(node);

            foreach (BasicNode childNode in node.Neighbors)
            {
                UseCroakOnNode(childNode);
            }
        }
        HasActedThisTurn = true;

        // Rehighlight so mite nodes will be highlighted
        if (!frogMove.HasMovedThisTurn) { frogMove.SetNeighborHighlights(true); }
    }

    private void UseCroakOnNode(BasicNode node)
    {
        if (node.NodeType == NodeHazard.Mites)
        {
            MiteNode tempNode = (MiteNode)node;
            tempNode.CroakUsedOnPad();
        }
        else if (node.NodeType == NodeHazard.Alligator)
        {
            AlligatorNode tempNode = (AlligatorNode)node;
            tempNode.CroakUsedOnPad();
        }
    }

    private void LayEgg()
    {
        BasicNode currentPad = gameObject.GetComponent<FrogMove>().GetLilyPadFrogOn();
        currentPad.SpawnObjectOnPad(eggPrefab);
        respawnNode = currentPad;
        HasActedThisTurn = true;
    }

    private void HopAgain()
    {
        HasActedThisTurn = true;
        frogMove.HopAgainUsed();

        // Rehighlight so new nodes will be highlighted
        frogMove.SetNeighborHighlights(true);
    }

    private void EndTurn()
    {
        TurnManager.Instance.AdvanceTurn();
        HasActedThisTurn = false;
    }

}
