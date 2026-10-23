using UnityEngine;
using Board.Input;

public class BoardUIRelay : MonoBehaviour
{
    private void OnEnable()
    {
        DeviceInputManager.FingerDown += OnFingerDown;
    }

    private void OnDisable()
    {
        DeviceInputManager.FingerDown -= OnFingerDown;
    }

    private void OnFingerDown(BoardContact contact)
    {
        if (InputHandler.Instance != null) return; // InputHandler handles Board UI taps in levels
        BoardUIClick.TryClick(contact.screenPosition);
    }
}