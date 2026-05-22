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
    private int currentSpeedMultiplier = 1;

    void Start()
    {
        Time.timeScale = 1f; 
        isPaused = false;
        currentSpeedMultiplier = 1;

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
            UpdateSpeedText();
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
        
        Time.timeScale = currentSpeedMultiplier;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void ToggleSpeedUp()
    {
        if (currentSpeedMultiplier == 1) currentSpeedMultiplier = 2;
        else if (currentSpeedMultiplier == 2) currentSpeedMultiplier = 4;
        else currentSpeedMultiplier = 1;

        if (speedUpBtnImage != null)
        {
            speedUpBtnImage.color = currentSpeedMultiplier == 1 ? normalColor : speedUpColor;
            UpdateSpeedText();
        }

        if (!isPaused)
        {
            Time.timeScale = currentSpeedMultiplier;
        }
    }

    private void UpdateSpeedText()
    {
        if (speedUpBtnImage != null)
        {
            var text = speedUpBtnImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "x" + currentSpeedMultiplier;
            }
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