using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mapSelectionPanel;

    private void Start()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }
    }

    public void Select10WaveMode()
    {
        // 0 = Finite Mode (10 Waves)
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.Save();
        ShowMapSelection();
    }

    public void SelectEndlessMode()
    {
        // 1 = Endless Mode
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.Save();
        ShowMapSelection();
    }

    private void ShowMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(true);
        }
    }

    // New method to close map panel and return to mode selection
    public void CloseMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }
    }

    public void LoadMap(string mapSceneName)
    {
        SceneManager.LoadScene(mapSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}