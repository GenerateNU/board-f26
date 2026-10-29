using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FrogMove : MonoBehaviour
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
    private FrogActions _frogActions;
    private bool _frogSucceeded = false;

    public int AssignedPlayer => _assignedPlayer;

    void Awake()
    {
        _currentLilyPad = _startingLilyPad;
    }
    void Start()
    {
        if (_currentLilyPad == null) Debug.LogWarning($"{name} has no starting lily pad assigned.");
        _frogActions = this.GetComponent<FrogActions>();

        if (!_frogActions)
        {
            Debug.LogWarning($"Missing frog actions for {name}, will not be able to cycle through turns.");
        }

        if (TurnManager.Instance != null)
            TurnManager.Instance.RegisterFrog(this, _assignedPlayer);
        else
            Debug.LogWarning($"{name}: no TurnManager in the scene, so this frog can't take turns.");
    }

    public BasicNode GetLilyPadFrogOn()
    {
        return _currentLilyPad;
    }

    public void StartTurn()
    {
        HasMovedThisTurn = false;
        SetNeighborHighlights(true);
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

    public void AddScore(int points)
    {
        Score += points;
    }

    public void DoAction(ActionType action)
    {
        if (_frogActions != null && _frogActions.IsActionAvailable(action))
        {
            _frogActions.DoAction(action);
        }
    }

    // Let the player move again this turn
    public void HopAgainUsed()
    {
        HasMovedThisTurn = false;
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

    public void RespawnAt(BasicNode node)
    {
        if (node == null) return;

        SetNeighborHighlights(false);
        if (_currentLilyPad != null) _currentLilyPad.CurrentFrogsOnPad -= 1;
        node.CurrentFrogsOnPad += 1;

        _currentLilyPad = node;
        transform.position = node.transform.position;
    }

    public void SetNeighborHighlights(bool on)
    {
        if (_currentLilyPad != null) _currentLilyPad.HighlightNeighbors(on);
    }

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

    public bool ShouldEndTurn(bool actionUsed)
    {
        return actionUsed && this.HasMovedThisTurn;
    }

    public bool IsValidMove(BasicNode clickedLilyPad)
    {
        if (clickedLilyPad == null || _currentLilyPad == null) return false;
        if (clickedLilyPad == _currentLilyPad) return false;
        if (!clickedLilyPad.IsTraversable) return false;
        if (!clickedLilyPad.CanBeEnteredFrom(_currentLilyPad)) return false;
        return CheckIfLilyPadIsNeighbor(clickedLilyPad);
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
        else if (_frogActions != null && ShouldEndTurn(_frogActions.HasActedThisTurn))
        {
            _frogActions.EndTurn();
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
}