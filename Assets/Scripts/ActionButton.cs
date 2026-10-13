using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{

    public InputHandler handler;
    public ActionType action;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => handler.OnActionButtonPressed(action));
    }
}
