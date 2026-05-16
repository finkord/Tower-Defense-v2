using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class WaveData
{
    public float duration = 10f;
    public int goblinEnemies = 5;
    public int orcEnemies = 2;
    public int ghostEnemies = 1;
}

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public WaveData[] waves;
    
    [Header("UI & Controls")]
    public Button startWaveButton;
    public TextMeshProUGUI waveText;
    
    [Header("Prefabs")]
    public GameObject goblinEnemiesPrefab;
    public GameObject orcEnemiesPrefab;
    public GameObject ghostEnemiesPrefab;
    
    [Header("Pathfinding")]
    public Transform[] wayPoints;
    
    private int currentWaveIndex = 0;
    private bool waveRunning = false;
    
    void Start()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(StartWave);
        }
        
        UpdateWaveText();
    }

    public void StartWave()
    {
        if (waveRunning || currentWaveIndex >= waves.Length) return;

        StartCoroutine(RunWave());
    }

    private IEnumerator RunWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        WaveData wave = waves[currentWaveIndex];
        float segmentDuration = wave.duration / 3f;

        // Spawn enemies sequentially 
        yield return StartCoroutine(SpawnGroup(goblinEnemiesPrefab, wave.goblinEnemies, segmentDuration));
        yield return StartCoroutine(SpawnGroup(orcEnemiesPrefab, wave.orcEnemies, segmentDuration));
        yield return StartCoroutine(SpawnGroup(ghostEnemiesPrefab, wave.ghostEnemies, segmentDuration));
        
        waveRunning = false;
        currentWaveIndex++;
        
        if (currentWaveIndex < waves.Length)
        {
            startWaveButton.interactable = true;
            UpdateWaveText();
        }
        else
        {
            // Optional: Handle all waves completed logic here
            if (waveText != null) waveText.text = "All Waves Cleared!";
        }
    }

    private IEnumerator SpawnGroup(GameObject prefab, int enemyCount, float duration)
    {
        if (enemyCount <= 0)
        {
            // Just wait the allocated time if there are no enemies of this type
            yield return new WaitForSeconds(duration);
            yield break;
        }

        float spawnDelay = duration / enemyCount;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (wayPoints == null || wayPoints.Length == 0) return;

        GameObject e = Instantiate(prefab, wayPoints[0].position, Quaternion.identity);
        
        if (e.TryGetComponent(out Enemy enemy))
        {
            enemy.waypoints = wayPoints;
        }
    }

    private void UpdateWaveText()
    {
        if (waveText != null && currentWaveIndex < waves.Length)
        {
            waveText.text = $"Wave - {currentWaveIndex + 1}";
        }
    }
}