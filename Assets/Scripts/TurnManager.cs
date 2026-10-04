using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;


public class TurnManager : MonoBehaviour
{

    //to-do: update code to handle player 3 (and 4? if needed)
    public enum TurnState { Player1, Player2, Player3, Enemy }


   // Easy global access for other scripts (e.g. TurnManager.Instance.CurrentTurn).
    public static TurnManager Instance { get; private set; }


    [Header("UI")]
    [Tooltip("TextMeshPro text that shows whose turn it is.")]
    [SerializeField] private TMP_Text turnText;


    [Header("Debug")]
    [Tooltip("Press Space to go to the next turn (for testing).")]
    [SerializeField] private bool enableSpaceToAdvance = true;


    public TurnState CurrentTurn { get; private set; } = TurnState.Player1;
   public event System.Action<TurnState> OnTurnChanged;
   private void Awake()
   {
       if (Instance != null & Instance != this)
       {
           Debug.LogWarning("Multiple TurnManagers found in the scene --> Pleasedestroy the duplicate");
           Destroy(gameObject);
           return;
       }
       Instance = this;
   }
   private void Start()
   {
       UpdateTurnUI();
       Debug.Log("Current Turn: " + CurrentTurn);
   }



   private void Update()
   {
       if (enableSpaceToAdvance && SpacePressedThisFrame())
       {
           AdvanceTurn();
       }
   }


   public void AdvanceTurn()
   {
       switch (CurrentTurn)
       {
           case TurnState.Player1:
               CurrentTurn = TurnState.Player2;
               break;
           case TurnState.Player2:
               CurrentTurn = TurnState.Enemy;
               break;
           case TurnState.Enemy:
               CurrentTurn = TurnState.Player1;
               break;
       }


       Debug.Log("Current Turn: " + CurrentTurn);
       UpdateTurnUI();
       OnTurnChanged?.Invoke(CurrentTurn);
   }


   private void UpdateTurnUI()
   {
       if (turnText == null) return;


       string label = CurrentTurn switch
       {
           TurnState.Player1 => "Player 1's Turn",
           TurnState.Player2 => "Player 2's Turn",
           TurnState.Enemy   => "Enemy's Turn",
           _ => CurrentTurn.ToString()
       };
       turnText.text = label;
   }


    private bool SpacePressedThisFrame()
    {
    return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

    }
}

















