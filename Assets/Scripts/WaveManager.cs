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
    public WaveData[] waves;
    public Button startWaveButton;
    
    public GameObject goblinEnemiesPrefab;
    public GameObject orcEnemiesPrefab;
    public GameObject ghostEnemiesPrefab;
    
    public Transform[] wayPoints;
    
    public TextMeshProUGUI waveText;
    
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
        for (int i = 0; i < wave.goblinEnemies; i++)
        {
            SpawnEnemy(goblinEnemiesPrefab);
            yield return new WaitForSeconds((wave.duration / 3f) / wave.goblinEnemies);
        }
        
        // Spawn Hard Enemies
        for (int i = 0; i < wave.orcEnemies; i++)
        {
            SpawnEnemy(orcEnemiesPrefab);
            yield return new WaitForSeconds((wave.duration / 3f) / wave.orcEnemies);
        }
        
        // Spawn Hard Enemies
        for (int i = 0; i < wave.ghostEnemies; i++)
        {
            SpawnEnemy(ghostEnemiesPrefab);
            yield return new WaitForSeconds((wave.duration / 3f) / wave.ghostEnemies);
        }
        
        // Final cooldown for the wave duration
        yield return new WaitForSeconds(wave.duration / 3f);
        
        waveRunning = false;
        startWaveButton.interactable = true;
        currentWaveIndex++;
        waveText.text = (currentWaveIndex + 1).ToString();
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