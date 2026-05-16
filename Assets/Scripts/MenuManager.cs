using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string gameSceneName = "MainScene"; // Change to your actual game scene name

    public void Start10WaveMode()
    {
        // 0 = Finite Mode (10 Waves)
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartEndlessMode()
    {
        // 1 = Endless Mode
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        // Works in built application
        Application.Quit();
        
        // Only for testing in Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}