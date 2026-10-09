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

    public bool HasActedThisTurn { get; set; } = false;

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
        foreach (BasicNode node in gameObject.GetComponent<FrogMove>().GetLilyPadFrogOn().Neighbors)
        {
            UseCroakOnMiteNode(node);

            foreach (BasicNode childNode in node.Neighbors)
            {
                UseCroakOnMiteNode(childNode);
            }
        }
        HasActedThisTurn = true;
    }

    private void UseCroakOnMiteNode(BasicNode node)
    {
        if (node.NodeType == NodeHazard.Mites)
        {
            MiteNode tempNode = (MiteNode)node;
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
        // We don't have any way of determining how many times a frog can hop / what a hop is yet.
        // Once we do, this should essentially do hops += 1.
        HasActedThisTurn = true;
    }

    private void EndTurn()
    {
        TurnManager.Instance.AdvanceTurn();
        HasActedThisTurn = false;
    }

}
