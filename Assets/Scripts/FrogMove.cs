using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class FrogMove : MonoBehaviour
{
    [SerializeField]
    private BasicNode _startingLilyPad;
    [Tooltip("Smaller numbers are faster speeds. 0.5 would make it 2x faster 2 would make it 1/2 slower.")]
    [SerializeField]
    private float _hopSpeed;
    [Tooltip("Lower numbers allow for movement to be smoother but may be more performance intensive.")]
    [SerializeField]
    private float _moveTimeStep;
    private BasicNode _currentLilyPad;

    // for sprint 2
    public bool HasMovedThisTurn { get; set; } = false;

    // These should probably be moved to a input handler/game controller script in the future as it should effect all frogs
    [SerializeField]
    private bool _preventInputMidHop;
    private bool _inAnimation;
    private InputAction _mouseClickAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentLilyPad = _startingLilyPad;
        _mouseClickAction = InputSystem.actions.FindAction("Attack");

        if (_mouseClickAction != null)
        {
            _mouseClickAction.performed += _ => MouseClicked();
        }
    }

    public BasicNode GetLilyPadFrogOn()
    {
        return _currentLilyPad;
    }

    private void MouseClicked()
    {
        if (_inAnimation && _preventInputMidHop) { return; }
        RaycastHit? hit = MakeRaycastFromMouse(Input.mousePosition);

        // This should only be done if the frog is not currently moving (we can't move a moving frog)
        if (!_inAnimation && hit != null) { CheckIfRayCastHitsLilyPad(hit.Value); }
    }

    // Makes a raycast from mouse and determines if any lilypads have been clicked
    private RaycastHit? MakeRaycastFromMouse(Vector2 mouseLocation)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseLocation.x, mouseLocation.y, 0));
        // Debug draw (uncomment if needed for debugging):
        // Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 50))
        {
            return hit;
        }

        return null;
    }

    // Checks if a raycast has hit a lilypad if it has move the frog to the lilypad
    private void CheckIfRayCastHitsLilyPad(RaycastHit hit)
    {
        BasicNode clickedLilypad = hit.transform.GetComponent<BasicNode>();

        // Exit if a lilypad is not clicked/if lilypad is not a neighbor
        if (!clickedLilypad) { return; }
        if (!CheckIfLilyPadIsNeighbor(clickedLilypad)) { return; }

        ChangeFrogLilyPad(hit.transform, clickedLilypad);
    }

    // Updates the current lilypad to the given one and moves the frog toward the given transform
    private void ChangeFrogLilyPad(Transform lilyPad, BasicNode newLilyPad)
    {
        if (!newLilyPad.IsTraversable) { return; }

        _inAnimation = true;

        _ = MoveFrog(lilyPad.position);
        _currentLilyPad.IsOccupied = false;
        newLilyPad.IsOccupied = true;

        // Update current lilypad
        _currentLilyPad = newLilyPad;
    }

    // Actual piece movement logic
    private async Task MoveFrog(Vector3 newPosition)
    {
        Vector3 originalPosition = this.gameObject.transform.position;
        float currentTime = 0f;

        while (this.gameObject.transform.position != newPosition)
        {
            this.gameObject.transform.position = Vector3.Lerp(originalPosition, newPosition, currentTime / _hopSpeed);
            await Awaitable.WaitForSecondsAsync(_moveTimeStep);
            currentTime += _moveTimeStep;
        }

        _inAnimation = false;
    }

    // Checks if the given lilypad is a neighbor of the clicked lilypad
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
