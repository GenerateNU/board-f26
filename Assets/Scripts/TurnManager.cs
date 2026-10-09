using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public enum TurnPhase { Player, Enemy }

    [System.Serializable]
    public class PlayerSlot
    {
        public int Number;   // 1-based: Player 1, Player 2, ...
        public string Name;

        public PlayerSlot(int number)
        {
            Number = number;
            Name = "Player " + number;
        }
    }

    public const int MaxPlayers = 3;

    public static TurnManager Instance { get; private set; }

    [Header("Players")]
    [Tooltip("How many players exist when the scene starts (handy for testing). More can join with AddPlayer().")]
    [SerializeField, Range(0, MaxPlayers)] private int startingPlayers = 1;

    [Header("UI")]
    [Tooltip("TextMeshPro text that shows whose turn it is.")]
    [SerializeField] private TMP_Text turnText;
    [Tooltip("Optional: the Add Player button. It greys out once the game is full.")]
    [SerializeField] private Button addPlayerButton;

    private readonly List<PlayerSlot> _players = new List<PlayerSlot>();
    private readonly Dictionary<int, FrogMove> _frogs = new Dictionary<int, FrogMove>(); // player number -> frog
    private int _currentPlayerIndex;
    private bool _started;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Player;
    public IReadOnlyList<PlayerSlot> Players => _players;
    public int PlayerCount => _players.Count;
    public bool CanAddPlayer => _players.Count < MaxPlayers;
    public int CurrentPlayerNumber =>
        CurrentPhase == TurnPhase.Player && _players.Count > 0 ? _players[_currentPlayerIndex].Number : 0;

    public bool IsPlayersTurn(int playerNumber) =>
        CurrentPhase == TurnPhase.Player && CurrentPlayerNumber == playerNumber;

    public string CurrentTurnLabel =>
        CurrentPhase == TurnPhase.Enemy ? "Enemy" : "Player " + CurrentPlayerNumber;

    public event System.Action OnTurnChanged;
    public event System.Action<PlayerSlot> OnPlayerAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple TurnManagers found; destroying the duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        for (int i = 0; i < startingPlayers; i++) AddPlayer();

        // With no players yet, sit on the enemy turn until someone joins.
        CurrentPhase = _players.Count > 0 ? TurnPhase.Player : TurnPhase.Enemy;
        _currentPlayerIndex = 0;
        _started = true;

        UpdateTurnUI();
        Debug.Log("Current Turn: " + CurrentTurnLabel);
        StartCurrentFrogTurn();
    }

    public void AddPlayer()
    {
        if (!CanAddPlayer)
        {
            Debug.Log($"Can't add another player: already at the max of {MaxPlayers}.");
            return;
        }

        var slot = new PlayerSlot(_players.Count + 1);
        _players.Add(slot);
        Debug.Log($"{slot.Name} joined ({_players.Count}/{MaxPlayers}).");

        if (addPlayerButton != null) addPlayerButton.interactable = CanAddPlayer;
        OnPlayerAdded?.Invoke(slot);
    }

    // Instead of a set player count, have it added way dynamically as players join.
    public void RegisterFrog(FrogMove frog, int playerNumber)
    {
        if (playerNumber < 1 || playerNumber > MaxPlayers)
        {
            Debug.LogWarning($"{frog.name} has player number {playerNumber}; it must be 1-{MaxPlayers}.");
            return;
        }

        if (_frogs.ContainsKey(playerNumber))
            Debug.LogWarning($"Player {playerNumber} already has a frog; replacing it with {frog.name}.");

        _frogs[playerNumber] = frog;

        // If the game is already running and it's this frog's turn, kick its turn off now.
        if (_started && IsPlayersTurn(playerNumber)) frog.StartTurn();
    }

    public void UnregisterFrog(FrogMove frog, int playerNumber)
    {
        if (_frogs.TryGetValue(playerNumber, out var registered) && registered == frog)
            _frogs.Remove(playerNumber);
    }

    
    public FrogMove GetCurrentFrog()
    {
        if (CurrentPhase != TurnPhase.Player) return null;
        _frogs.TryGetValue(CurrentPlayerNumber, out var frog);
        return frog;
    }

    public void AdvanceTurn()
    {
        GetCurrentFrog()?.EndTurn();

        if (CurrentPhase == TurnPhase.Player)
        {
            _currentPlayerIndex++;
            if (_currentPlayerIndex >= _players.Count) CurrentPhase = TurnPhase.Enemy;
        }
        else // Enemy
        {
            if (_players.Count > 0)
            {
                CurrentPhase = TurnPhase.Player;
                _currentPlayerIndex = 0;
            }
            // No players yet: stay on the enemy turn.
        }

        Debug.Log("Current Turn: " + CurrentTurnLabel);
        UpdateTurnUI();
        StartCurrentFrogTurn();
        OnTurnChanged?.Invoke();
    }

    private void StartCurrentFrogTurn()
    {
        GetCurrentFrog()?.StartTurn();
    }


    private void UpdateTurnUI()
    {
        if (turnText == null) return;
        turnText.text = CurrentTurnLabel + "'s Turn";
    }
}