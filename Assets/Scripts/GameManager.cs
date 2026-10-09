using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private FrogMove frog; 
    [SerializeField] private FishEnemy Fish;
    [SerializeField] private GameObject WinText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TurnManager.Instance.OnTurnChanged += CheckFrogWin;
    }

    void CheckFrogWin()
    {
        // if(frog.GetLilyPadFrogOn().isGoalPad) //isGoalPad does not currently exist
        // {
        //     Debug.Log("You Win!");
        //     WinText.SetActive(true);
        // }
    }



}
