using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class FrogMove : MonoBehaviour
{
    [SerializeField]
    private LilyPadNode _startingLilyPad; // can probably be condensed into one field with currentLilyPad but I don't want currentLilyPad to be serializeable so they're separate
    [Tooltip("Smaller numbers are faster speeds. 0.5 would make it 2x faster 2 would make it 1/2 slower.")]
    [SerializeField]
    private float _hopSpeed;
    [Tooltip("Lower numbers allow for movement to be smoother but may be more performance intensive.")]
    [SerializeField]
    private float _moveTimeStep;
    private LilyPadNode _currentLilyPad;

    // TODO: Move all below fields in an inputHandler/GameController script so we can have multiple frogs and it will be easier to prevent clicks from all of them.
    // This can likely be brought together more smoothly if we have all of the beginning parts as we may want to not allow inputs during other animations (enemyAI)
    // and having this in a separate script will allow PlayerChecking to be smoother. Alongside that I recommend giving each Frog an Identifier script.
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

    public LilyPadNode GetLilyPadFrogOn()
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
        LilyPadNode clickedLilypad = hit.transform.GetComponent<LilyPadNode>();

        // Exit if a lilypad is not clicked/if lilypad is not a neighbor
        if (!clickedLilypad) { return; }
        if (!CheckIfLilyPadIsNeighbor(clickedLilypad)) { return; }

        ChangeFrogLilyPad(hit.transform, clickedLilypad);
    }

    // Updates the current lilypad to the given one and moves the frog toward the given transform
    private void ChangeFrogLilyPad(Transform lilyPad, LilyPadNode newLilyPad)
    {
        // Update current lilypad
        _currentLilyPad = newLilyPad;
        _inAnimation = true;

        _ = MoveFrog(lilyPad.position);
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
    private bool CheckIfLilyPadIsNeighbor(LilyPadNode clickedLilyPad)
    {
        // It may be sufficient enough to check in the clickedLilyPad is our current lilyPad is there
        // however if we ever want to implement one way lilyPads/in general for code readability I think it's better
        // to just give lilyPads ids.
        int lilyPadID = clickedLilyPad.NodeID;
        IReadOnlyList<LilyPadNode> neighbors = _currentLilyPad.Neighbors;

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
