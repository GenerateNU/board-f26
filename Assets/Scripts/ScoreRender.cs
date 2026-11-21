using TMPro;
using UnityEngine;


public class ScoreRender : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI score;

    public int scoreValue;

    public void initiateScore(int score)
    {
        UpdateScoreText(score);
    }

    void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateScoreText(int scoreNumber)
    {
        scoreValue = scoreNumber;
        score.text = scoreNumber.ToString();
    }
}
