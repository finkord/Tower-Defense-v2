using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class WaveData
{
    public float duration = 10f;
    public int easyEnemies = 5;
    public int hardEnemies = 2;
}

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public Button startWaveButton;
    
    public GameObject easyEnemiesPrefab;
    public GameObject hardEnemiesPrefab;
    
    public Transform[] wayPoints;
    
    private int currentWaveIndex = 0;
    private bool waveRunning = false;
    
    void Start()
    {
        if (startWaveButton != null)
            startWaveButton.onClick.AddListener(StartWave);
    }

    public void StartWave()
    {
        if (waveRunning || currentWaveIndex >= waves.Length) return;

        StartCoroutine(RunWave());
    }

    IEnumerator RunWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        WaveData wave = waves[currentWaveIndex];

        // Spawn Easy Enemies
        for (int i = 0; i < wave.easyEnemies; i++)
        {
            SpawnEnemy(easyEnemiesPrefab);
            yield return new WaitForSeconds((wave.duration / 3f) / wave.easyEnemies);
        }
        
        // Spawn Hard Enemies
        for (int i = 0; i < wave.hardEnemies; i++)
        {
            SpawnEnemy(hardEnemiesPrefab);
            yield return new WaitForSeconds((wave.duration / 3f) / wave.hardEnemies);
        }
        
        // Final cooldown for the wave duration
        yield return new WaitForSeconds(wave.duration / 3f);
        
        waveRunning = false;
        startWaveButton.interactable = true;
        currentWaveIndex++;
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (wayPoints.Length == 0) return;

        GameObject e = Instantiate(prefab, wayPoints[0].position, Quaternion.identity);
        
        // Ensure the Enemy script exists on the prefab
        if (e.TryGetComponent(out Enemy enemy))
        {
            enemy.waypoints = wayPoints;
        }
    }
}