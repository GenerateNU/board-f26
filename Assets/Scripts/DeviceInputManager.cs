using System.Collections.Generic;
using System;
using Board.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DeviceInputManager : MonoBehaviour
{
    public static DeviceInputManager Instance;
    public static Action<BoardContact> PieceUp, PieceDown, PieceMoved, FingerDown, FingerUp, FingerMoved;
    private Mouse _boardMouse;

    void Awake()
    {
        if(Instance && Instance != this)
        {
            Debug.Log("already has an isntance of Device Input Manager");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        _boardMouse = InputSystem.AddDevice<Mouse>("Board Finger Mouse");
    }

    private void OnDestroy()
    {
        if (_boardMouse != null)
        {
            InputSystem.RemoveDevice(_boardMouse);
        }
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get all active contacts (fingers and Pieces) each frame.
        BoardContact[] GlyphContacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
        foreach (var contact in GlyphContacts)
        {
            switch (contact.phase)
            {
                case BoardContactPhase.Began: PieceDown?.Invoke(contact); break;
                case BoardContactPhase.Moved: PieceMoved?.Invoke(contact); break;
                case BoardContactPhase.Ended: PieceUp?.Invoke(contact); break;
                case BoardContactPhase.Canceled: PieceUp?.Invoke(contact); break;
            }
        }

        BoardContact[] FingerContacts = BoardInput.GetActiveContacts(BoardContactType.Finger);
        Queue<Vector2> _pendingFingerClicks = new Queue<Vector2>();
        foreach (var contact in FingerContacts)
        {
            if (contact.phase == BoardContactPhase.Began)
            {
                _pendingFingerClicks.Enqueue(contact.screenPosition);
            }
        }
        ProcessPendingFingerClicks(_pendingFingerClicks);
        //Debug.Log("fingers on screen: " + FingerContacts.Length);
    }

    private void ProcessPendingFingerClicks(Queue<Vector2> pendingClicks)
    {
        if (_boardMouse == null) return;

        while (pendingClicks.Count > 0)
        {
            //Debug.Log("Processing pending finger click");
            Vector2 position = pendingClicks.Dequeue();
            MouseState pressState = new MouseState { position = position };
            InputSystem.QueueStateEvent(_boardMouse, pressState.WithButton(MouseButton.Left));
            InputSystem.QueueStateEvent(_boardMouse, new MouseState { position = position });
        }
    }
}
