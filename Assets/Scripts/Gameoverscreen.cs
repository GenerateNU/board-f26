using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private string wonMessage = "You Won!";
    [SerializeField] private string lostMessage = "You Died";

    [Tooltip("Used by the optional Main Menu button.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void Show(bool won)
    {
        if (resultText != null) resultText.text = won ? wonMessage : lostMessage;
        gameObject.SetActive(true);
    }

    // Hook to the Restart Level button. Reloads the current level from the start
    // (the player count chosen in the menu is kept).
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}