using System;
using System.Collections;
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

    private bool _endTurnPending;

    void Awake()
    {
        frogMove = GetComponent<FrogMove>();
    }

    void Start()
    {
        if (TurnManager.Instance != null) TurnManager.Instance.OnTurnChanged += OnTurnChanged;
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null) TurnManager.Instance.OnTurnChanged -= OnTurnChanged;
    }

    // Reset only when a NEW turn actually starts for this frog,
    // not when EndTurn is requested (the turn change can be blocked or delayed).
    private void OnTurnChanged()
    {
        if (IsMyTurn())
        {
            HasActedThisTurn = false;
            _endTurnPending = false;
        }
    }

    private bool IsMyTurn()
    {
        return TurnManager.Instance != null
            && frogMove != null
            && TurnManager.Instance.GetCurrentFrog() == frogMove;
    }

    public bool IsActionAvailable(ActionType action)
    {
        // Nothing is available outside this frog's turn, or once it has asked to end its turn.
        if (!IsMyTurn() || _endTurnPending) return false;

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
        if (!IsActionAvailable(action))
        {
            Debug.Log($"{name}: {action} isn't available right now.");
            return;
        }

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
        BasicNode currentPad = frogMove.GetLilyPadFrogOn();
        if (currentPad == null) return;

        foreach (BasicNode node in currentPad.Neighbors)
        {
            node.CroakUsedOnPad();

            foreach (BasicNode childNode in node.Neighbors)
            {
                childNode.CroakUsedOnPad();
            }
        }
        HasActedThisTurn = true;

        // Rehighlight so mite nodes will be highlighted
        if (!frogMove.HasMovedThisTurn) { frogMove.SetNeighborHighlights(true); }
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }
    }

    private void LayEgg()
    {
        BasicNode currentPad = frogMove.GetLilyPadFrogOn();
        if (currentPad == null) return;

        currentPad.SpawnObjectOnPad(eggPrefab);
        respawnNode = currentPad;
        HasActedThisTurn = true;
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }
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

    // Ends this frog's turn. If an animation (like a hop) is still playing,
    // waits for it to finish instead of being silently ignored.
    public void EndTurn()
    {
        if (_endTurnPending || !IsMyTurn()) return;
        _endTurnPending = true;
        StartCoroutine(EndTurnWhenReady());
    }

    private IEnumerator EndTurnWhenReady()
    {
        while (InputHandler.Instance != null && InputHandler.Instance.InAnimation)
        {
            yield return null;
        }

        // Still our turn? (Something else may have advanced it meanwhile.)
        if (IsMyTurn() && TurnManager.Instance != null)
        {
            TurnManager.Instance.AdvanceTurn();
        }
    }
}