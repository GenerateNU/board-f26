using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ActionType
{
    Croak,
    LayEgg,
    HopAgain,
    EndTurn
}

public class FrogUnit : MonoBehaviour
{
    [Header("Turn")]
    [Tooltip("Which player (1-3) controls this frog.")]
    [SerializeField, Range(1, TurnManager.MaxPlayers)]
    private int _assignedPlayer = 1;

    // One hop per turn. Reset in StartTurn().
    public bool HasMovedThisTurn { get; private set; }
    public int Score { get; private set; }

    [Header("Movement")]
    [SerializeField]
    private BasicNode _startingLilyPad;
    [Tooltip("Smaller numbers are faster speeds. 0.5 would make it 2x faster 2 would make it 1/2 slower.")]
    [SerializeField]
    private float _hopSpeed;
    [Tooltip("Lower numbers allow for movement to be smoother but may be more performance intensive.")]
    [SerializeField]
    private float _moveTimeStep;
    private BasicNode _currentLilyPad;
    private bool _isHopping;
    private bool _frogSucceeded = false;

    [Header("Actions")]
    [Tooltip("Egg prefab (with the DecoyEgg script). Laying it lures the fish away from frogs.")]
    [SerializeField]
    private GameObject eggPrefab;

    // This frog's egg that hasn't been eaten yet (one at a time).
    private DecoyEgg _activeEgg;
    public bool HasActiveEgg => _activeEgg != null;

    public bool HasActedThisTurn { get; set; } = false;

    private bool _endTurnPending;

    public int AssignedPlayer => _assignedPlayer;

    void Awake()
    {
        _currentLilyPad = _startingLilyPad;
    }
    void Start()
    {
        if (_currentLilyPad == null) Debug.LogWarning($"{name} has no starting lily pad assigned.");

        if (TurnManager.Instance != null)
            TurnManager.Instance.RegisterFrog(this, _assignedPlayer);
        else
            Debug.LogWarning($"{name}: no TurnManager in the scene, so this frog can't take turns.");
    }

    public void KillFrog()
    {
        // won't show who called it. This log does.
        Debug.Log($"KillFrog called on {name} (Player {_assignedPlayer})", this);
        Destroy(this.gameObject);

        if (gameObject.scene.isLoaded)
        {
            string padName = _currentLilyPad != null ? _currentLilyPad.name : "no pad";
            Debug.Log($"{name} (Player {_assignedPlayer}) destroyed on {padName}", this);
        }

        // Don't leave input locked if the frog is destroyed mid-hop.
        if (_isHopping) InputHandler.Instance?.EndAnimation();

        // The whole scene is unloading (Restart / Main Menu): this isn't a death or a win,
        // so don't report it. Otherwise Restart would trigger Game Over during the reload.
        if (!gameObject.scene.isLoaded) return;

        if (_currentLilyPad != null) _currentLilyPad.CurrentFrogsOnPad -= 1;
        SetNeighborHighlights(false);
        TurnManager.Instance?.UnregisterFrog(this, _assignedPlayer, _frogSucceeded);
    }

    // Getters and Setters
    public BasicNode GetLilyPadFrogOn()
    {
        return _currentLilyPad;
    }

    public void SetNeighborHighlights(bool on)
    {
        if (_currentLilyPad != null) _currentLilyPad.HighlightNeighbors(on);
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    // Turn Logic
    private bool IsMyTurn()
    {
        return TurnManager.Instance != null && TurnManager.Instance.IsPlayersTurn(_assignedPlayer);
    }

    public void StartTurn()
    {
        HasMovedThisTurn = false;
        HasActedThisTurn = false;
        _endTurnPending = false;
        SetNeighborHighlights(true);
    }

    public bool ShouldEndTurn(bool actionUsed)
    {
        return actionUsed && this.HasMovedThisTurn;
    }

    public void EndTurn()
    {
        SetNeighborHighlights(false);
        HasMovedThisTurn = true; // can't act again until StartTurn() resets it

        if (_currentLilyPad != null && _currentLilyPad.TryGetComponent<FlyPickup>(out var flyPickup))
        {
            flyPickup.Collect(this);
        }
    }

    public void EndPlayerTurn()
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
        // Without this check, a late end-turn could skip the NEXT player's turn.
        if (IsMyTurn())
        {
            TurnManager.Instance.AdvanceTurn();
        }
    }

    // Helper Functions for Checking
    public bool IsValidMove(BasicNode clickedLilyPad)
    {
        if (clickedLilyPad == null || _currentLilyPad == null) return false;
        if (clickedLilyPad == _currentLilyPad) return false;
        if (!clickedLilyPad.IsTraversable) return false;
        if (!clickedLilyPad.CanBeEnteredFrom(_currentLilyPad)) return false;
        return CheckIfLilyPadIsNeighbor(clickedLilyPad);
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

    // Checks if the given lilypad is a neighbor of the current lilypad
    private bool CheckIfLilyPadIsNeighbor(BasicNode clickedLilyPad)
    {
        IReadOnlyList<BasicNode> neighbors = _currentLilyPad.Neighbors;

        for (int i = 0; i < neighbors.Count; i++)
        {
            if (clickedLilyPad == neighbors[i])
            {
                return true;
            }
        }

        return false;
    }

    // Movement
    public bool TryHopTo(BasicNode clickedLilyPad)
    {
        var turnManager = TurnManager.Instance;
        if (turnManager == null || turnManager.CurrentPlayerNumber != _assignedPlayer) return false; // not our turn
        if (HasMovedThisTurn || _isHopping) return false;                                             // already moved
        if (!IsValidMove(clickedLilyPad)) return false;                                               // not a legal pad

        SetNeighborHighlights(false); // clear the glow before _currentLilyPad changes
        HasMovedThisTurn = true;
        ChangeFrogLilyPad(clickedLilyPad);

        return true;
    }

    // Updates the current lilypad to the given one and moves the frog toward it
    private void ChangeFrogLilyPad(BasicNode newLilyPad)
    {
        if (!newLilyPad.IsTraversable) { return; }

        _isHopping = true;
        InputHandler.Instance?.BeginAnimation();

        _ = MoveFrog(newLilyPad.transform.position);
        _currentLilyPad.CurrentFrogsOnPad -= 1;
        newLilyPad.CurrentFrogsOnPad += 1;

        // Update current lilypad
        _currentLilyPad = newLilyPad;
    }

    // Actual piece movement logic
    private async Task MoveFrog(Vector3 newPosition)
    {
        Vector3 originalPosition = transform.position;
        float currentTime = 0f;

        while (transform.position != newPosition)
        {
            transform.position = Vector3.Lerp(originalPosition, newPosition, currentTime / _hopSpeed);
            await Awaitable.WaitForSecondsAsync(_moveTimeStep);
            if (this == null) return; // frog was destroyed mid-hop
            currentTime += _moveTimeStep;
        }

        _isHopping = false;
        InputHandler.Instance?.EndAnimation();
        SetNeighborHighlights(false);

        // Normal move/first move, check if action is done to end game/turn
        if (_currentLilyPad.NodeType == NodeHazard.Winning)
        {
            _frogSucceeded = true;
            KillFrog();
            TurnManager.Instance.AdvanceTurn();
        }
        else if (ShouldEndTurn(HasActedThisTurn))
        {
            EndPlayerTurn();
        }
    }

    // Actions
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
                EndPlayerTurn();
                break;
            default:
                return;
        }
    }

    // croak, lay egg, hop again etc.
    private void Croak()
    {
        BasicNode currentPad = GetLilyPadFrogOn();
        if (currentPad == null) return;

        currentPad.FrogCroakedOnPad(this);

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
        if (!HasMovedThisTurn) { SetNeighborHighlights(true); }
        if (ShouldEndTurn(HasActedThisTurn)) { EndPlayerTurn(); }
    }

    // Lays a decoy egg on the frog's current pad. While any eggs exist, the fish chases eggs instead of frogs.
    private void LayEgg()
    {
        BasicNode currentPad = GetLilyPadFrogOn();
        if (currentPad == null) return;
        if (eggPrefab == null)
        {
            Debug.LogWarning($"{name}: no Egg Prefab assigned on FrogUnit.");
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
        if (ShouldEndTurn(HasActedThisTurn)) { EndPlayerTurn(); }
    }

    private void HopAgain()
    {
        // Don't let the player use HopAgain unless it is their second action.
        if (GetLilyPadFrogOn().NodeType == NodeHazard.Mites || !HasMovedThisTurn)
        {
            return;
        }

        HasActedThisTurn = true;
        HasMovedThisTurn = false;

        // Rehighlight so new nodes will be highlighted
        SetNeighborHighlights(true);
        if (ShouldEndTurn(HasActedThisTurn)) { EndPlayerTurn(); }
    }
}