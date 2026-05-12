using UnityEngine;

public class TowerSelectionUI : MonoBehaviour
{
    public static GameObject SelectedTowerPrefab;

    public void SelectedTower(GameObject towerPrefab)
    {
        if (towerPrefab == SelectedTowerPrefab)
        {
            SelectedTowerPrefab = null;
            return;
        }

        // Get Tower component from prefab
        Tower towerScript = towerPrefab.GetComponentInChildren<Tower>();

        // Check if component and its ScriptableObject data exist
        if (towerScript != null && towerScript.data != null)
        {
            // Access price through the ScriptableObject
            if (towerScript.data.towerPrice <= CoinManager.instance.coins)
            {
                SelectedTowerPrefab = towerPrefab;
            }
        }
    }
}