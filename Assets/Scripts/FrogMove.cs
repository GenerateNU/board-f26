using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FrogMove : MonoBehaviour
{
    [SerializeField]
    private LilyPadNode _startingLilyPad; // can probably be condensed into one field with currentLilyPad but I don't want currentLilyPad to be serializeable so they're separate
    [SerializeField]
    private float _hopSpeed;
    [SerializeField]
    private bool _preventInputMidHop;
    private LilyPadNode _currentLilyPad;
    // In the future (depending on how game design goes) it may be better to move all mouse logic into the same script
    // so all input logic is together and just have a controller which has a reference to this script
    private InputAction _mouseClickAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentLilyPad = _startingLilyPad;
        // If multiple scripts end up searching for this action Attack string should be made into global variables
        // to make changes easier
        _mouseClickAction = InputSystem.actions.FindAction("Attack");

        if (_mouseClickAction != null)
        {
            _mouseClickAction.performed += _ => MoveFrog();
        }
    }

    public LilyPadNode GetLilyPadFrogOn()
    {
        return _currentLilyPad;
    }

    private void MoveFrog()
    {
        MakeRaycastFromMouse(Input.mousePosition);
    }

    private void MakeRaycastFromMouse(Vector2 mouseLocation)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseLocation.x, mouseLocation.y, 0));
        // Debug draw (uncomment if needed for debugging):
        // Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, 50))
        {
            LilyPadNode clickedLilypad = hit.transform.GetComponent<LilyPadNode>();

            // Exit if a lilypad is not clicked/if lilypad is not a neighbor
            if (!clickedLilypad) { return; }
            if (!CheckIfLilyPadIsNeighbor(clickedLilypad)) { return; }

            ChangeFrogLilyPad(clickedLilypad);
        }
    }

    private void ChangeFrogLilyPad(LilyPadNode newLilyPad)
    {

    }

    private bool CheckIfLilyPadIsNeighbor(LilyPadNode clickedLilyPad)
    {
        // It may be sufficient enough to check in the clickedLilyPad is our current lilyPad is there
        // however if we ever want to implement one way lilyPads/in general for code readability I think it's better
        // to just give lilyPads ids.
        int lilyPadID = clickedLilyPad.NodeID;
        List<LilyPadNode> neighbors = _currentLilyPad.Neighbors;

        for (int i = 0; i < neighbors; i++)
        {
            if (lilyPadID == neighbors[i].NodeID)
            {
                return true;
            }
        }

        return false;
    }
}
