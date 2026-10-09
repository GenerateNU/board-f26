using System;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{

    public FrogActions frog;
    public ActionType action;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => frog.DoAction(action));
    }

    // Update is called once per frame
    void Update()
    {
        //Eventually this should be updated to get the current player's frog.
        //However, right now we cannot do that, so this is limited to one manually-selected frog.
        gameObject.GetComponent<Button>().interactable = frog.IsActionAvailable(action);
    }
}
