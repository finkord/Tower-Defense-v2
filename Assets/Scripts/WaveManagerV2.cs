using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum WaveMode
{
    Finite,
    Endless,
    PvPHotSeat
}

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject prefab;
    public EnemyData data; 
    [HideInInspector] public List<GameObject> pool = new List<GameObject>(); 
}

public class WaveManagerV2 : MonoBehaviour
{
    [Header("Wave Mode")]
    public WaveMode waveMode = WaveMode.Finite;
    public int maxWaves = 10; 

    [Header("Difficulty Scaling")]
    public float healthScalePerWave = 0.2f; 
    public float speedScalePerWave = 0.05f; 
    public float rewardScalePerWave = 0.1f; 

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
    public TextMeshProUGUI gameWinText;

    public int currentWaveIndex { get; private set; } = 0;
    private bool waveRunning = false;

    private void TriggerGameWin()
    {
        Time.timeScale = 0f; 

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }

        if (gameWinText != null && waveMode == WaveMode.PvPHotSeat)
        {
            gameWinText.text = "Defender Wins!";
        }
    }
    
    private void Awake()
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
        else if (mode == 2)
        {
            waveMode = WaveMode.PvPHotSeat;
            maxWaves = 10;
        }
    }

    void Start()
    {
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
        if ((waveMode == WaveMode.Finite || waveMode == WaveMode.PvPHotSeat) && currentWaveIndex >= maxWaves) return;

        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        waveRunning = true;
        if (startWaveButton != null) startWaveButton.interactable = false;

        List<EnemySpawnConfig> enemiesToSpawn;
        if (waveMode == WaveMode.PvPHotSeat && PvPManager.Instance != null)
        {
            enemiesToSpawn = new List<EnemySpawnConfig>(PvPManager.Instance.pvpWaveQueue);
        }
        else
        {
            int currentBudget = CalculateBudget(currentWaveIndex);
            enemiesToSpawn = GenerateWave(currentBudget);
        }
        
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
        
        while (spawnedEnemies.Exists(enemy => enemy != null && enemy.activeInHierarchy))
        {
            yield return new WaitForSeconds(0.5f);
        }

        waveRunning = false;
        currentWaveIndex++;
        
        if (waveMode == WaveMode.Endless || currentWaveIndex < maxWaves)
        {
            if (waveMode == WaveMode.PvPHotSeat && PvPManager.Instance != null)
            {
                PvPManager.Instance.StartAttackerTurn();
            }
            else
            {
                if (startWaveButton != null) startWaveButton.interactable = true;
            }
            UpdateWaveText();
        }
        else
        {
            if (waveText != null) waveText.text = "All Waves Cleared!";
            TriggerGameWin();
        }
    }

    public int CalculateBudget(int waveIndex)
    {
        float multiplier = budgetCurve.Evaluate(waveIndex);
        return Mathf.RoundToInt(initialBudget * multiplier);
    }

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

        foreach (GameObject pooledObj in config.pool)
        {
            if (pooledObj != null && !pooledObj.activeInHierarchy)
            {
                enemyObj = pooledObj;
                break;
            }
        }

        if (enemyObj == null)
        {
            enemyObj = Instantiate(config.prefab, wayPoints[0].position, Quaternion.identity);
            config.pool.Add(enemyObj);
        }
        else
        {
            enemyObj.transform.position = wayPoints[0].position;
            enemyObj.SetActive(true);
        }
        
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

    [Header("Debug Settings")]
    public GameObject debugPanel;
    public TextMeshProUGUI debugText;

    private void Update()
    {
        if (debugPanel != null && debugPanel.activeInHierarchy && debugText != null)
        {
            float hMult = 1f + (currentWaveIndex * healthScalePerWave);
            float sMult = 1f + (currentWaveIndex * speedScalePerWave);
            float rMult = 1f + (currentWaveIndex * rewardScalePerWave);
            int currentBudget = CalculateBudget(currentWaveIndex);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("--- WaveManagerV2 Debug ---");
            sb.AppendLine($"Wave Mode: {waveMode}");
            sb.AppendLine($"Current Wave: {currentWaveIndex}");
            sb.AppendLine($"Max Waves: {maxWaves}");
            sb.AppendLine($"Wave Running: {waveRunning}");
            sb.AppendLine($"Health Mult: {hMult}");
            sb.AppendLine($"Speed Mult: {sMult}");
            sb.AppendLine($"Reward Mult: {rMult}");
            sb.AppendLine($"Current Budget: {currentBudget}");
            sb.AppendLine("--- Enemy Pools ---");

            if (availableEnemies != null)
            {
                foreach(var e in availableEnemies)
                {
                    int activeCount = 0;
                    if (e.pool != null)
                    {
                        foreach(var obj in e.pool)
                        {
                            if (obj != null && obj.activeInHierarchy) activeCount++;
                        }
                        string prefabName = e.prefab != null ? e.prefab.name : "Unknown";
                        sb.AppendLine($"{prefabName}: {activeCount} active / {e.pool.Count} total");
                    }
                }
            }
            debugText.text = sb.ToString();
        }
    }

    public void ToggleDebugPanel(bool isOn)
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(isOn);
        }
    }
}