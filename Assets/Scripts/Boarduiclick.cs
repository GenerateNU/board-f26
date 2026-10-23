using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class BoardUIClick
{
    private static readonly List<RaycastResult> _results = new List<RaycastResult>();

    public static bool TryClick(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        var pointerData = new PointerEventData(eventSystem)
        {
            position = screenPosition,
            pressPosition = screenPosition,
            button = PointerEventData.InputButton.Left,
            clickCount = 1,
            clickTime = Time.unscaledTime,
            eligibleForClick = true
        };

        _results.Clear();
        eventSystem.RaycastAll(pointerData, _results); // sorted, topmost first
        if (_results.Count == 0) return false;

        // Walk from the topmost UI element down until we find something clickable.
        foreach (RaycastResult result in _results)
        {
            GameObject clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(result.gameObject);
            if (clickTarget == null) continue;

            pointerData.pointerCurrentRaycast = result;
            pointerData.pointerPressRaycast = result;
            pointerData.pointerPress = clickTarget;
            pointerData.rawPointerPress = result.gameObject;

            ExecuteEvents.Execute(clickTarget, pointerData, ExecuteEvents.pointerClickHandler);
            return true;
        }

        // Touched UI that isn't clickable (text, background image): not a button press.
        return false;
    }
}