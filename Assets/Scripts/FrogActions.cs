using System;
using System.Collections.Generic;
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
            node.CroakUsedOnPad();

            foreach (BasicNode childNode in node.Neighbors)
            {
                childNode.CroakUsedOnPad();
            }
        }
        EnemyTurnManager.Instance.CroakUsed();
        HasActedThisTurn = true;

        // Rehighlight so mite nodes will be highlighted
        if (!frogMove.HasMovedThisTurn) { frogMove.SetNeighborHighlights(true); }
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }
    }

    private void LayEgg()
    {
        return;
        // We should still use this code for egg refactoring just take out the respawnNode part
        /*BasicNode currentPad = gameObject.GetComponent<FrogMove>().GetLilyPadFrogOn();
        currentPad.SpawnObjectOnPad(eggPrefab);
        respawnNode = currentPad;
        HasActedThisTurn = true;
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }*/
    }

    private void HopAgain()
    {
        // Don't let the player use HopAgain unless it is their second action.
        if (frogMove.GetLilyPadFrogOn().NodeType == NodeHazard.Mites || !frogMove.HasMovedThisTurn)
        {
            return;
        }

        HasActedThisTurn = true;
        frogMove.HopAgainUsed();

        // Rehighlight so new nodes will be highlighted
        frogMove.SetNeighborHighlights(true);
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }
    }

    public void EndTurn()
    {
        TurnManager.Instance.AdvanceTurn();
        HasActedThisTurn = false;
    }

}
