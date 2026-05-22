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
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.Save();
        ShowMapSelection();
    }

    public void SelectEndlessMode()
    {
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