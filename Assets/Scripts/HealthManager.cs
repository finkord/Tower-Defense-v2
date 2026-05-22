using UnityEngine;
using TMPro;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public int health = 100;
    public TextMeshProUGUI healthTxt;

    [Header("UI Panels")]
    public GameObject gameOverPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        healthTxt.text = health.ToString();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void UpdateHealth(int changeAmount)
    {
        health += changeAmount;

        if (health < 0) health = 0;

        healthTxt.text = health.ToString();

        if (health <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Time.timeScale = 0f; 

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}