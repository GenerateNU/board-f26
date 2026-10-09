using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    [Header("Raycasting")]
    [Tooltip("Camera used for click raycasts. Defaults to Camera.main.")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _maxRayDistance = 50f;

    [Header("Input")]
    [Tooltip("Name of the click action in the project-wide Input Actions.")]
    [SerializeField] private string _clickActionName = "Attack";
    [Tooltip("Ignore clicks (and Space) while a frog is mid-hop.")]
    [SerializeField] private bool _preventInputMidAnimation = true;
    private bool _inAnimation;
    private InputAction _mouseClickAction;
    private bool _ownsClickAction; // true if we had to create our own action as a fallback

    [Header("Debug")]
    [Tooltip("Press Space to advance the turn (for testing).")]
    [SerializeField] private bool _enableSpaceToAdvance = true;

    public bool InAnimation => _inAnimation;
    public bool IsInputBlocked => _preventInputMidAnimation && _inAnimation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple InputHandlers found; destroying the duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_camera == null) _camera = Camera.main;

        _mouseClickAction = InputSystem.actions != null ? InputSystem.actions.FindAction(_clickActionName) : null;
        if (_mouseClickAction == null)
        {
            Debug.LogWarning($"No '{_clickActionName}' action found in the project-wide Input Actions; using left mouse button instead.");
            _mouseClickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
            _ownsClickAction = true;
        }
    }

    private void OnEnable()
    {
        if (_mouseClickAction == null) return;
        _mouseClickAction.performed += OnMouseClick;
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        if (_mouseClickAction == null) return;
        _mouseClickAction.performed -= OnMouseClick;
        // Don't disable the shared project action; other scripts may use it.
        if (_ownsClickAction) _mouseClickAction.Disable();
    }

    private void OnDestroy()
    {
        if (_ownsClickAction) _mouseClickAction?.Dispose();
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_enableSpaceToAdvance
            && Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame
            && !IsInputBlocked
            && TurnManager.Instance != null)
        {
            TurnManager.Instance.AdvanceTurn();
        }
    }


    // On Mouse CLICK

    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (IsInputBlocked) return;

        var turnManager = TurnManager.Instance;
        if (turnManager == null || turnManager.CurrentPhase != TurnManager.TurnPhase.Player) return;

        LilyPadNode clickedLilyPad = GetLilyPadUnderMouse();
        if (clickedLilyPad == null) return;

        FrogMove frog = turnManager.GetCurrentFrog();
        if (frog == null)
        {
            Debug.Log($"{turnManager.CurrentTurnLabel} has no frog assigned, so the click was ignored.");
            return;
        }

        frog.TryHopTo(clickedLilyPad);
    }

    // Makes a raycast from the mouse and returns the lily pad it hit, if any
    private LilyPadNode GetLilyPadUnderMouse()
    {
        if (_camera == null || Mouse.current == null) return null;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        //Debugging
        Debug.DrawRay(ray.origin, ray.direction * _maxRayDistance, Color.yellow, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance))
        {
            return hit.transform.GetComponent<LilyPadNode>();
        }
        return null;
    }


    // call this when a hop animation starts.
    public void BeginAnimation() => _inAnimation = true;

    // call this when a hop animation finishes.
    public void EndAnimation() => _inAnimation = false;
}