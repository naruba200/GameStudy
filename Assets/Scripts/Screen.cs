using UnityEngine;
using UnityEngine.SceneManagement;

public class Screen : MonoBehaviour
{
    private static bool hasStartedThisSession;

    // Tên scene (set trong Inspector)
    public string gameSceneName = "Game";
    public string menuSceneName = "MainMenu";
    public GameObject dialogueManager;

    [Header("Start Screen")]
    public GameObject startScreen;
    public bool pauseGameWhenStartScreenVisible = true;

    private void Start()
    {
        if (hasStartedThisSession)
        {
            if (startScreen != null)
            {
                startScreen.SetActive(false);
            }

            Time.timeScale = 1f;
            return;
        }

        if (pauseGameWhenStartScreenVisible && startScreen != null && startScreen.activeSelf)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // Bấm nút Bắt đầu: tắt màn hình start và vào game
    public void StartGame()
    {
        hasStartedThisSession = true;

        if (startScreen != null)
        {
            startScreen.SetActive(false);
        }

        dialogueManager.GetComponent<DialogueManager>().StartDialogue();
    }

    // Retry - chơi lại
    public void RetryGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Về menu chính
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // Thoát game
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Time.timeScale = 1f;
        Application.Quit();
    }
}
