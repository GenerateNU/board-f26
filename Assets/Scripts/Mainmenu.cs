using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Exact name of the level scene (must be in Build Settings).")]
    [SerializeField] private string levelSceneName = "Level_01";

    [Header("Panels")]
    [Tooltip("Optional: the 1/2/3 player buttons. If empty, PLAY starts with 1 player.")]
    [SerializeField] private GameObject playerSelectPanel;
    [Tooltip("Optional: placeholder How To Play panel.")]
    [SerializeField] private GameObject howToPlayPanel;
    [Tooltip("Optional: placeholder Settings panel (gear).")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        CloseAllPanels();
    }

    // PLAY button
    public void OnPlayPressed()
    {
        if (playerSelectPanel == null)
        {
            SelectPlayers(1); // no panel set up yet: just start
            return;
        }
        ShowOnly(playerSelectPanel);
    }

    // HOW TO PLAY button
    public void OnHowToPlayPressed() => ShowOnly(howToPlayPanel);

    // Gear button
    public void OnSettingsPressed() => ShowOnly(settingsPanel);

    // Back buttons on any panel
    public void CloseAllPanels() => ShowOnly(null);

    // 1 / 2 / 3 Player

    public void SelectPlayers(int count)
    {
        GameSettings.PlayerCount = Mathf.Clamp(count, 1, TurnManager.MaxPlayers);
        Debug.Log($"Starting {levelSceneName} with {GameSettings.PlayerCount} player(s).");
        SceneManager.LoadScene(levelSceneName);
    }

    private void ShowOnly(GameObject panel)
    {
        if (playerSelectPanel != null) playerSelectPanel.SetActive(panel == playerSelectPanel);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(panel == howToPlayPanel);
        if (settingsPanel != null) settingsPanel.SetActive(panel == settingsPanel);
    }

}