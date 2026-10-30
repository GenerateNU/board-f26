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
    [Tooltip("Egg prefab (with the DecoyEgg script). Laying it lures the fish away from frogs.")]
    [SerializeField]
    private GameObject eggPrefab;
    private FrogMove frogMove;

    // This frog's egg that hasn't been eaten yet (one at a time).
    private DecoyEgg _activeEgg;
    public bool HasActiveEgg => _activeEgg != null;

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
                return !HasActedThisTurn && !HasActiveEgg; // one uneaten egg per frog at a time
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
            if (node == null) continue; // adding some safety checks so nothing explodes during all hands
            node.CroakUsedOnPad();

            if (node.Neighbors == null) continue;
            foreach (BasicNode childNode in node.Neighbors)
            {
                if (childNode == null) continue; // safety check 2
                childNode.CroakUsedOnPad();
            }
        }

        // safety check 3 just in case
        if (EnemyTurnManager.Instance != null)
        {
            EnemyTurnManager.Instance.CroakUsed();
        }
        else
        {
            Debug.LogWarning("EnemyTurnManager.Instance not found in scene");
        }

        HasActedThisTurn = true;

        // Rehighlight so mite nodes will be highlighted
        if (!frogMove.HasMovedThisTurn) { frogMove.SetNeighborHighlights(true); }
        if (frogMove.ShouldEndTurn(HasActedThisTurn)) { EndTurn(); }
    }

    // Lays a decoy egg on the frog's current pad. While any eggs exist, the fish chases eggs instead of frogs.
    private void LayEgg()
    {
        BasicNode currentPad = frogMove.GetLilyPadFrogOn();
        if (currentPad == null) return;
        if (eggPrefab == null)
        {
            Debug.LogWarning($"{name}: no Egg Prefab assigned on FrogActions.");
            return;
        }

        // Spawn the decoy on this pad and tell it where it is.
        GameObject eggObject = Instantiate(eggPrefab, currentPad.transform.position, Quaternion.identity);
        DecoyEgg egg = eggObject.GetComponent<DecoyEgg>();
        if (egg == null) egg = eggObject.AddComponent<DecoyEgg>(); // safety if the prefab is missing it
        egg.Init(currentPad, this);
        _activeEgg = egg;

        Debug.Log($"{name} laid a decoy egg on {currentPad.name}");
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