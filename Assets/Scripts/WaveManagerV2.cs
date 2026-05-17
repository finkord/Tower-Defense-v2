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
    [HideInInspector] public List<GameObject> pool = new List<GameObject>(); // Store instantiated enemies here
}

public class WaveManagerV2 : MonoBehaviour
{
    [Header("Wave Mode")]
    public WaveMode waveMode = WaveMode.Finite;
    public int maxWaves = 10; 

    [Header("Difficulty Scaling")]
    public float healthScalePerWave = 0.2f; // 20% health increase per wave
    public float speedScalePerWave = 0.05f; // 5% speed increase per wave
    public float rewardScalePerWave = 0.1f; // 10% reward increase per wave

    [Header("Budget Settings")]
    public float initialBudget = 10f;
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
        List<EnemySpawnConfig> enemiesToSpawn = GenerateWave(currentBudget);
        List<GameObject> spawnedEnemies = new List<GameObject>();

        foreach (EnemySpawnConfig enemyConfig in enemiesToSpawn)
        {
            GameObject newEnemy = SpawnEnemy(enemyConfig, currentWaveIndex);
            if (newEnemy != null)
            {
                spawnedEnemies.Add(newEnemy);
            }
            yield return new WaitForSeconds(spawnInterval);
        }
        
        // Wait until all tracked enemies are either destroyed OR inactive (returned to pool)
        while (spawnedEnemies.Exists(enemy => enemy != null && enemy.activeInHierarchy))
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

    // Changed return type to List<EnemySpawnConfig> to track which pool to use
    private List<EnemySpawnConfig> GenerateWave(int budget)
    {
        List<EnemySpawnConfig> waveList = new List<EnemySpawnConfig>();
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
            waveList.Add(chosen);
            remainingBudget -= chosen.data.reward;
        }

        return waveList;
    }

    private GameObject SpawnEnemy(EnemySpawnConfig config, int waveIndex)
    {
        if (wayPoints == null || wayPoints.Length == 0) return null;

        GameObject enemyObj = null;

        // 1. Try to find an inactive enemy in the pool
        foreach (GameObject pooledObj in config.pool)
        {
            if (pooledObj != null && !pooledObj.activeInHierarchy)
            {
                enemyObj = pooledObj;
                break;
            }
        }

        // 2. If no inactive enemy found, instantiate a new one and add it to the pool
        if (enemyObj == null)
        {
            enemyObj = Instantiate(config.prefab, wayPoints[0].position, Quaternion.identity);
            config.pool.Add(enemyObj);
        }
        else
        {
            // Move existing enemy to start position and activate
            enemyObj.transform.position = wayPoints[0].position;
            enemyObj.SetActive(true);
        }
        
        // 3. Reset enemy state
        if (enemyObj.TryGetComponent(out Enemy enemy))
        {
            enemy.waypoints = wayPoints;
            float hMult = 1f + (waveIndex * healthScalePerWave);
            float sMult = 1f + (waveIndex * speedScalePerWave);
            float rMult = 1f + (waveIndex * rewardScalePerWave);
            enemy.Init(hMult, sMult, rMult); 
        }

        return enemyObj;
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