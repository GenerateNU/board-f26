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
    [Tooltip("Players to use when the level is played directly in the editor. " +
    "When started from the Main Menu, the menu's choice is used instead.")]
    [SerializeField, Range(0, MaxPlayers)] private int startingPlayers = 1;

    [Header("UI")]
    [Tooltip("TextMeshPro text that shows whose turn it is.")]
    [SerializeField] private TMP_Text turnText;
    [Tooltip("Optional: the Add Player button. It greys out once the game is full.")]
    [SerializeField] private Button addPlayerButton;

    [Header("Game Over")]
    [Tooltip("The GameOver_Canvas prefab (with the GameOverScreen script on its root).")]
    [SerializeField] private GameOverScreen gameOverScreenPrefab;

    private readonly List<PlayerSlot> _players = new List<PlayerSlot>();
    private readonly Dictionary<int, FrogMove> _frogs = new Dictionary<int, FrogMove>(); // player number -> frog
    private int _currentPlayerIndex;
    private int _frogsSucceeded;
    private bool _started;
    private bool _gameOver;

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Player;
    public IReadOnlyList<PlayerSlot> Players => _players;
    public int PlayerCount => _players.Count;
    public bool CanAddPlayer => _players.Count < MaxPlayers;
    public bool IsGameOver => _gameOver;
    public int CurrentPlayerNumber =>
        CurrentPhase == TurnPhase.Player && _players.Count > 0 ? _players[_currentPlayerIndex].Number : 0;

    public bool IsPlayersTurn(int playerNumber) =>
        !_gameOver && CurrentPhase == TurnPhase.Player && CurrentPlayerNumber == playerNumber;

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
        // Use the Main Menu's choice if there was one, otherwise the Inspector value.
        int playerCount = GameSettings.HasPlayerCount ? GameSettings.PlayerCount : startingPlayers;
        playerCount = Mathf.Clamp(playerCount, 0, MaxPlayers);
        for (int i = 0; i < playerCount; i++) AddPlayer();

        // Frogs for players who aren't in this game get removed.
        RemoveFrogsWithoutPlayers();

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

        // Frog registered after the game started for a player who isn't playing: remove it.
        if (_started && playerNumber > _players.Count)
        {
            frog.gameObject.SetActive(false);
            return;
        }

        if (_frogs.ContainsKey(playerNumber))
            Debug.LogWarning($"Player {playerNumber} already has a frog; replacing it with {frog.name}.");

        _frogs[playerNumber] = frog;

        // If the game is already running and it's this frog's turn, kick its turn off now.
        if (_started && IsPlayersTurn(playerNumber)) frog.StartTurn();
    }

    // Hides frogs whose player number is higher than the number of players in this game.
    private void RemoveFrogsWithoutPlayers()
    {
        var toRemove = new List<int>();
        foreach (var kvp in _frogs)
        {
            if (kvp.Key > _players.Count) toRemove.Add(kvp.Key);
        }

        foreach (int playerNumber in toRemove)
        {
            FrogMove frog = _frogs[playerNumber];
            _frogs.Remove(playerNumber); // remove first so it doesn't count toward win/loss
            if (frog != null) frog.gameObject.SetActive(false);
        }
    }

    public void UnregisterFrog(FrogMove frog, int playerNumber, bool frogOutcomePositive)
    {
        if (_frogs.TryGetValue(playerNumber, out var registered) && registered == frog)
        {
            _frogs.Remove(playerNumber);
        }

        if (frogOutcomePositive) { _frogsSucceeded++; }
        if (_frogs.Count == 0)
        {
            this.GameOver(_frogsSucceeded > 0);
        }
    }

    // If any frog makes it to the end count it as a win for now
    public void GameOver(bool won)
    {
        if (_gameOver) return;
        _gameOver = true;
        _frogsSucceeded = 0;

        Debug.Log(won ? "Game Over: won" : "Game Over: lost");

        if (gameOverScreenPrefab != null)
            Instantiate(gameOverScreenPrefab).Show(won);
        else
            Debug.LogWarning("No GameOver_Canvas prefab assigned on TurnManager.");
    }

    public FrogMove GetCurrentFrog()
    {
        if (CurrentPhase != TurnPhase.Player) return null;
        _frogs.TryGetValue(CurrentPlayerNumber, out var frog);
        return frog;
    }

    public List<FrogMove> GetAllFrogs()
    {
        List<FrogMove> allFrogs = new List<FrogMove>();
        foreach (var kvp in _frogs)
        {
            allFrogs.Add(kvp.Value);
        }
        return allFrogs;
    }

    public void AdvanceTurn()
    {
        if (_gameOver) { return; }

        // Never change turns while a frog is still hopping.
        if (InputHandler.Instance != null && InputHandler.Instance.InAnimation) { return; }

        if (_frogs.Count == 0) { return; }
        GetCurrentFrog()?.EndTurn(); // FrogMove.EndTurn() locks the frog until its next turn

        if (CurrentPhase == TurnPhase.Player)
        {
            _currentPlayerIndex++;
            if (_currentPlayerIndex >= _players.Count)
            {
                CurrentPhase = TurnPhase.Enemy;
            }

            AdjustPlayerIndexToAlivePlayer();
        }
        else // Enemy
        {
            if (_players.Count > 0)
            {
                CurrentPhase = TurnPhase.Player;
                _currentPlayerIndex = 0;
                AdjustPlayerIndexToAlivePlayer();
            }
            // No players yet: stay on the enemy turn.
        }

        Debug.Log("Current Turn: " + CurrentTurnLabel);

        UpdateTurnUI();
        StartCurrentFrogTurn();
        OnTurnChanged?.Invoke();
    }

    private void AdjustPlayerIndexToAlivePlayer()
    {
        if (GetCurrentFrog() == null)
        {
            while (_currentPlayerIndex < _players.Count && GetCurrentFrog() == null)
            {
                _currentPlayerIndex++;
            }

            if (_currentPlayerIndex >= _players.Count) CurrentPhase = TurnPhase.Enemy;
        }
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