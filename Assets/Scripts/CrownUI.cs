using UnityEngine;

public class CrownUI : MonoBehaviour
{
    [SerializeField] ScoreRender[] playerIcons;

    Transform currentParent = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("go to leading")]
    public void GoToLeadingPlayer()
    {
        int max = 0;
        foreach (ScoreRender icon in playerIcons)
        {
            if (icon.scoreValue > max)
            {
                max = icon.scoreValue;
                currentParent = icon.transform;
            }
        }

        if (currentParent) transform.SetParent(currentParent, false);
    }
}
