using UnityEngine;
using Board.Input;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
public class ButtonClick : MonoBehaviour
{
    [SerializeField] Camera _camera;

    [SerializeField] Bounds bounds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DeviceInputManager.FingerDown += OnFingerDown;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnFingerDown(BoardContact contact)
    {

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = _camera.ViewportToScreenPoint(contact.screenPosition)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
        //Debug.Log(results[0].gameObject.name);
        results[0].gameObject.GetComponent<Button>().onClick.Invoke();
        }
    }

}
