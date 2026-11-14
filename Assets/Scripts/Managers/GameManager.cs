using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    // [SerializeField] private FrogUnit frog;
    // [SerializeField] private FishEnemy Fish;
    // [SerializeField] private GameObject WinText;
    public List<TurnManager.PlayerSlot> Winners = new List<TurnManager.PlayerSlot>();
    public static GameManager Instance;



    void Awake()
    {
        if (Instance != null || this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        TurnManager.Instance.OnTurnChanged += CheckFrogWin;
        TurnManager.Instance.OnGameEnd += recordWinners;
    }

    private void recordWinners(TurnManager.PlayerSlot slot)
    {
        Winners.Add(slot);
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
