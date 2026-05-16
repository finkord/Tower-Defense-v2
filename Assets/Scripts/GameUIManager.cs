using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Slider volumeSlider;

    [Header("Speed Up Settings")]
    public Image speedUpBtnImage;
    public Color normalColor;
    public Color speedUpColor;

    private bool isPaused = false;
    private bool isSpeedUp = false;

    void Start()
    {
        Time.timeScale = 1f; 
        isPaused = false;
        isSpeedUp = false;

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        if (speedUpBtnImage != null)
        {
            speedUpBtnImage.color = normalColor;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        
        // Restore time scale based on speedUp state
        Time.timeScale = isSpeedUp ? 2f : 1f;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void ToggleSpeedUp()
    {
        isSpeedUp = !isSpeedUp;

        // Update button color instantly
        if (speedUpBtnImage != null)
        {
            speedUpBtnImage.color = isSpeedUp ? speedUpColor : normalColor;
        }

        // Apply time scale change only if the game is not paused
        if (!isPaused)
        {
            Time.timeScale = isSpeedUp ? 2f : 1f;
        }
    }

    public void ExitGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }

    private void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}