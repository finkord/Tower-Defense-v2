using UnityEngine;

public class TowerSelectionUI : MonoBehaviour
{
    public static GameObject SelectedTowerPrefab;

    public void SelectedTower(GameObject towerPrefab)
    {
        if (towerPrefab == SelectedTowerPrefab)
        {
            SelectedTowerPrefab=null;
            return;
        }
        
        SelectedTowerPrefab = towerPrefab;
    }
}