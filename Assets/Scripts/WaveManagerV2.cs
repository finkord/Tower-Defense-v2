using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum WaveMode
{
    Finite,
    Endless
}

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject prefab;
    public EnemyData data; 
}

public class WaveManagerV2 : MonoBehaviour
{
    [Header("Wave Mode")]
    public WaveMode waveMode = WaveMode.Finite;
    public int maxWaves = 10; 

    [Header("Budget Settings")]
    public float initialBudget = 10f;
    [Tooltip("X-axis: Wave Index. Y-axis: Budget Multiplier")]
    public AnimationCurve budgetCurve = AnimationCurve.Linear(0, 1, 20, 10); 

    [Header("Enemies Pool")]
    public List<EnemySpawnConfig> availableEnemies;
    
    [Header("Spawn Settings")]
    public float spawnInterval = 1f; 
    public Transform[] wayPoints;
    
    [Header("UI & Controls")]
    public Button startWaveButton;
    public TextMeshProUGUI waveText;
    
    [Header("UI Panels")] 
    public GameObject gameWinPanel;

    private int currentWaveIndex = 0;
    private bool waveRunning = false;

    private void TriggerGameWin()
    {
        Time.timeScale = 0f; 

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }
    }
    
    void Start()
    {
        int mode = PlayerPrefs.GetInt("GameMode", 0);
        
        if (mode == 0)
        {
            waveMode = WaveMode.Finite;
            maxWaves = 10;
        }
        else if (mode == 1)
        {
            waveMode = WaveMode.Endless;
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartWave);
        }
        
        UpdateWaveText();
        
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(false);
        }
    }

    public void StartWave()
    {
        if (waveRunning) return;
        if (waveMode == WaveMode.Finite && currentWaveIndex >= maxWaves) return;

        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        waveRunning = true;
        if (startWaveButton != null) startWaveButton.interactable = false;

        int currentBudget = CalculateBudget(currentWaveIndex);
        List<GameObject> enemiesToSpawn = GenerateWave(currentBudget);
        List<GameObject> spawnedEnemies = new List<GameObject>();

        // Spawn generated enemies and track them
        foreach (GameObject enemyPrefab in enemiesToSpawn)
        {
            GameObject newEnemy = SpawnEnemy(enemyPrefab);
            if (newEnemy != null)
            {
                spawnedEnemies.Add(newEnemy);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        
        // Wait until all tracked enemies are destroyed
        while (spawnedEnemies.Exists(enemy => enemy != null))
        {
            yield return new WaitForSeconds(0.5f);
        }

        waveRunning = false;
        currentWaveIndex++;
        
        if (waveMode == WaveMode.Endless || currentWaveIndex < maxWaves)
        {
            if (startWaveButton != null) startWaveButton.interactable = true;
            UpdateWaveText();
        }
        else
        {
            if (waveText != null) waveText.text = "All Waves Cleared!";
            TriggerGameWin();
        }
    }

    private int CalculateBudget(int waveIndex)
    {
        float multiplier = budgetCurve.Evaluate(waveIndex);
        return Mathf.RoundToInt(initialBudget * multiplier);
    }

    private List<GameObject> GenerateWave(int budget)
    {
        List<GameObject> waveList = new List<GameObject>();
        int remainingBudget = budget;

        int minCost = int.MaxValue;
        foreach (var enemy in availableEnemies)
        {
            if (enemy.data.reward < minCost)
                minCost = enemy.data.reward;
        }

        while (remainingBudget >= minCost)
        {
            List<EnemySpawnConfig> affordableEnemies = new List<EnemySpawnConfig>();
            
            foreach (var enemy in availableEnemies)
            {
                if (enemy.data.reward <= remainingBudget)
                {
                    affordableEnemies.Add(enemy);
                }
            }

            if (affordableEnemies.Count == 0) break;

            EnemySpawnConfig chosen = affordableEnemies[Random.Range(0, affordableEnemies.Count)];
            waveList.Add(chosen.prefab);
            remainingBudget -= chosen.data.reward;
        }

        return waveList;
    }

    // Changed return type from void to GameObject
    private GameObject SpawnEnemy(GameObject prefab)
    {
        if (wayPoints == null || wayPoints.Length == 0) return null;

        GameObject e = Instantiate(prefab, wayPoints[0].position, Quaternion.identity);
        
        if (e.TryGetComponent(out Enemy enemy))
        {
            enemy.waypoints = wayPoints;
        }

        return e;
    }

    private void UpdateWaveText()
    {
        if (waveText == null) return;

        if (waveMode == WaveMode.Endless)
        {
            waveText.text = $"Wave - {currentWaveIndex + 1}";
        }
        else
        {
            waveText.text = $"Wave - {currentWaveIndex + 1} / {maxWaves}";
        }
    }
}