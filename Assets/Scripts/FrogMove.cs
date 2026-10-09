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

    public int AssignedPlayer => _assignedPlayer;

    void Start()
    {
        _currentLilyPad = _startingLilyPad;
        if (_currentLilyPad == null) Debug.LogWarning($"{name} has no starting lily pad assigned.");

        if (TurnManager.Instance != null)
            TurnManager.Instance.RegisterFrog(this, _assignedPlayer);
        else
            Debug.LogWarning($"{name}: no TurnManager in the scene, so this frog can't take turns.");
    }

    private void OnDestroy()
    {
        // Don't leave input locked if the frog is destroyed mid-hop.
        if (_isHopping) InputHandler.Instance?.EndAnimation();
        TurnManager.Instance?.UnregisterFrog(this, _assignedPlayer);
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
    }

    private void SetNeighborHighlights(bool on)
    {
        if (_currentLilyPad != null) _currentLilyPad.HighlightNeighbors(on);
    }

    public bool TryHopTo(LilyPadNode clickedLilyPad)
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

    public bool IsValidMove(LilyPadNode clickedLilyPad)
    {
        if (clickedLilyPad == null || _currentLilyPad == null) return false;
        if (clickedLilyPad == _currentLilyPad) return false;
        if (!clickedLilyPad.IsTraversable) return false;
        return CheckIfLilyPadIsNeighbor(clickedLilyPad);
    }

    // Updates the current lilypad to the given one and moves the frog toward it
    private void ChangeFrogLilyPad(LilyPadNode newLilyPad)
    {
        _isHopping = true;
        InputHandler.Instance?.BeginAnimation();

        _ = MoveFrog(newLilyPad.transform.position);

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
    }

    // Checks if the given lilypad is a neighbor of the current lilypad
    private bool CheckIfLilyPadIsNeighbor(BasicNode clickedLilyPad)
    {
        int lilyPadID = clickedLilyPad.NodeID;
        IReadOnlyList<BasicNode> neighbors = _currentLilyPad.Neighbors;

        for (int i = 0; i < neighbors.Count; i++)
        {
            if (lilyPadID == neighbors[i].NodeID)
            {
                return true;
            }
        }

        return false;
    }
}