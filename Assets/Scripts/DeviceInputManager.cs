using System.Collections.Generic;
using System.Text;
using System;
using Board.Input;
using UnityEngine;
using UnityEngine.UI;

public class DeviceInputManager : MonoBehaviour
{
    public static DeviceInputManager Instance;
    public static Action<BoardContact> PieceUp, PieceDown, PieceMoved, FingerDown, FingerUp, FingerMoved;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(Instance && Instance != this)
        {
            Debug.Log("already has an isntance of Device Input Manager");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
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
        foreach (var contact in FingerContacts)
        {
            switch (contact.phase)
            {
                case BoardContactPhase.Began: FingerDown?.Invoke(contact); break;
                case BoardContactPhase.Moved: FingerMoved?.Invoke(contact); break;
                case BoardContactPhase.Ended: FingerUp?.Invoke(contact); break;
                case BoardContactPhase.Canceled: FingerUp?.Invoke(contact); break;
            }
        }

        //Debug.Log("fingers on screen: " + FingerContacts.Length);
    }
}
