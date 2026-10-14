using UnityEngine;
using UnityEngine.InputSystem;
using Board.Input;
using System;
using System.Collections.Generic;

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

    [Tooltip("ON: Board hardware (finger touches + game pieces). OFF: mouse / regular touchscreen + on-screen buttons.")]
    [SerializeField] private bool useBoardInput = true;

    public bool InAnimation => _inAnimation;
    public bool IsInputBlocked => _preventInputMidAnimation && _inAnimation;

    [SerializeField] FrogActions[] frogs;
    [SerializeField] GlyphActionPair[] GlyphActionPairs;

    [Serializable]
    public struct GlyphActionPair
    {
        public int id;
        public ActionType action;
    }

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
            Debug.LogWarning($"No '{_clickActionName}' action found in the project-wide Input Actions; using pointer press (mouse/touch/pen) instead.");
            _mouseClickAction = new InputAction("Click", InputActionType.Button, "<Pointer>/press");
            _ownsClickAction = true;
        }
    }

    private void OnEnable()
    {
        if (useBoardInput)
        {
            DeviceInputManager.FingerDown += OnFingerDown;
            DeviceInputManager.PieceDown += OnPieceDown;
        }

        if (_mouseClickAction == null) return;
        if (!useBoardInput) _mouseClickAction.performed += OnMouseClick;
        _mouseClickAction.Enable();
    }

    private void OnDisable()
    {
        // Static events: always unsubscribe, or after Restart they'd still call this destroyed object.
        DeviceInputManager.FingerDown -= OnFingerDown;
        DeviceInputManager.PieceDown -= OnPieceDown;

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

    // Returns the current player's frog if input is allowed right now, otherwise null.
    private FrogMove GetFrogIfInputAllowed()
    {
        if (IsInputBlocked) return null;

        var turnManager = TurnManager.Instance;
        if (turnManager == null || turnManager.IsGameOver || turnManager.CurrentPhase != TurnManager.TurnPhase.Player) return null;

        FrogMove frog = turnManager.GetCurrentFrog();
        if (frog == null)
            Debug.Log($"{turnManager.CurrentTurnLabel} has no frog, so the input was ignored.");
        return frog;
    }

    // On-screen action buttons (mouse mode only)
    public void OnActionButtonPressed(ActionType action)
    {
        if (useBoardInput) { return; }
        FrogMove frog = GetFrogIfInputAllowed();
        if (frog != null) frog.DoAction(action);
    }

    private void HopToLilyPad(Vector2 screenPosition)
    {
        FrogMove frog = GetFrogIfInputAllowed();
        if (frog == null) return;

        BasicNode clickedLilyPad = GetLilyPadAtScreenPoint(screenPosition);
        if (clickedLilyPad == null) return;

        frog.TryHopTo(clickedLilyPad);
    }

    // Mouse click (or regular touchscreen / pen tap)
    private void OnMouseClick(InputAction.CallbackContext context)
    {
        if (useBoardInput || Pointer.current == null) { return; }
        HopToLilyPad(Pointer.current.position.ReadValue());
    }

    // Board finger touch
    private void OnFingerDown(BoardContact contact)
    {
        if (!useBoardInput) { return; }
        HopToLilyPad(contact.screenPosition);
    }

    // Board game piece placed
    void OnPieceDown(BoardContact contact)
    {
        if (!useBoardInput) { return; }

        FrogMove frog = GetFrogIfInputAllowed();
        if (frog == null) return;

        int index = Array.FindIndex(GlyphActionPairs, p => p.id == contact.glyphId);
        if (index < 0)
        {
            Debug.Log($"No action is mapped to glyph {contact.glyphId}.");
            return;
        }

        frog.DoAction(GlyphActionPairs[index].action);
    }

    // Raycasts from a screen point (mouse, finger, or pen) and returns the lily pad it hit, if any
    private BasicNode GetLilyPadAtScreenPoint(Vector2 screenPoint)
    {
        if (_camera == null) return null;

        Ray ray = _camera.ScreenPointToRay(screenPoint);
        //Debugging
        Debug.DrawRay(ray.origin, ray.direction * _maxRayDistance, Color.yellow, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance))
        {
            return hit.transform.GetComponent<BasicNode>();
        }
        return null;
    }

    // call this when a hop animation starts or animation which is meant to prevent input starts.
    public void BeginAnimation() => _inAnimation = true;

    // call this when a hop animation finishes or animation which is meant to prevent input starts.
    public void EndAnimation() => _inAnimation = false;
}