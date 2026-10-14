using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Exact name of the level scene (must be in Build Settings).")]
    [SerializeField] private string levelSceneName = "Level_01";

    [Tooltip("Dropdown for choosing the number of players. Options are filled in automatically.")]
    [SerializeField] private TMP_Dropdown playerCountDropdown;

    [SerializeField, Range(1, TurnManager.MaxPlayers)] private int defaultPlayerCount = 1;

    private void Start()
    {
        if (playerCountDropdown == null) return;

        var options = new List<string>();
        for (int i = 1; i <= TurnManager.MaxPlayers; i++)
        {
            options.Add(i == 1 ? "1 Player" : i + " Players");
        }

        playerCountDropdown.ClearOptions();
        playerCountDropdown.AddOptions(options);
        playerCountDropdown.value = defaultPlayerCount - 1;
        playerCountDropdown.RefreshShownValue();
    }

    public void StartGame()
    {
        int count = playerCountDropdown != null ? playerCountDropdown.value + 1 : defaultPlayerCount;
        GameSettings.PlayerCount = Mathf.Clamp(count, 1, TurnManager.MaxPlayers);

        Debug.Log($"Starting {levelSceneName} with {GameSettings.PlayerCount} player(s).");
        SceneManager.LoadScene(levelSceneName);
    }
}