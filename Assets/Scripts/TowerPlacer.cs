using UnityEngine;

using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public Tilemap placementMap;
    public Tilemap nonPlacementMap;
    public Tilemap decorationsMap;

    public GameObject ghostPrefab;
    
    private Dictionary<Vector3Int, GameObject> placedTowers = new Dictionary<Vector3Int, GameObject>();
    private GameObject ghostInstance;
    private GameObject currentGhostPrefab;

    private GameObject CreateVisualClone(GameObject prefab)
    {
        GameObject clone = new GameObject(prefab.name + "_Ghost");
        
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            SpriteRenderer newSr = clone.AddComponent<SpriteRenderer>();
            newSr.sprite = sr.sprite;
            newSr.color = sr.color;
            newSr.sortingLayerID = sr.sortingLayerID;
            newSr.sortingOrder = sr.sortingOrder;
        }

        foreach (Transform child in prefab.transform)
        {
            GameObject childClone = CreateVisualClone(child.gameObject);
            childClone.transform.SetParent(clone.transform);
            childClone.transform.localPosition = child.localPosition;
            childClone.transform.localRotation = child.localRotation;
            childClone.transform.localScale = child.localScale;
        }

        return clone;
    }
    
    bool HasDecorationInCell(Vector3Int placementCellPos)
    {
        if (decorationsMap == null) return false;

        Vector3 cellCenter = placementMap.GetCellCenterWorld(placementCellPos);
        Vector3 cellSize = placementMap.layoutGrid.cellSize;

        float inset = 0.05f;
        Vector3 minWorld = cellCenter - (cellSize / 2f) + new Vector3(inset, inset, 0);
        Vector3 maxWorld = cellCenter + (cellSize / 2f) - new Vector3(inset, inset, 0);

        Vector3Int minDecCell = decorationsMap.WorldToCell(minWorld);
        Vector3Int maxDecCell = decorationsMap.WorldToCell(maxWorld);

        for (int x = minDecCell.x; x <= maxDecCell.x; x++)
        {
            for (int y = minDecCell.y; y <= maxDecCell.y; y++)
            {
                if (decorationsMap.HasTile(new Vector3Int(x, y, 0)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void Update()
    {
        HandleTowerDeletion();
        HandlePlacementClick();
        HandlePlacementHover();
    }

    void HandleTowerDeletion()
    {
        if (GameManager.Instance != null && 
            (GameManager.Instance.CurrentState != GameState.Preparation && 
             GameManager.Instance.CurrentState != GameState.WaveRunning || 
             GameManager.Instance.IsPaused)) return;

        if (Input.GetMouseButtonDown(1)) 
        {
            if (TowerSelectionUI.SelectedTowerPrefab != null)
            {
                TowerSelectionUI.SelectedTowerPrefab = null;
                return;
            }

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

            if (placedTowers.TryGetValue(cellPos, out GameObject towerToDelete))
            {
                Tower towerComponent = towerToDelete.GetComponentInChildren<Tower>();
                if (towerComponent != null && towerComponent.data != null)
                {
                    CoinManager.instance.UpdateCoins(towerComponent.data.towerPrice / 2);
                }
                
                Destroy(towerToDelete);
                placedTowers.Remove(cellPos);
            }
        }
    }

    void HandlePlacementClick()
    {
        if (GameManager.Instance != null && 
            (GameManager.Instance.CurrentState != GameState.Preparation && 
             GameManager.Instance.CurrentState != GameState.WaveRunning || 
             GameManager.Instance.IsPaused)) return;

        if(!Input.GetMouseButtonDown(0)) return;
        if (TowerSelectionUI.SelectedTowerPrefab == null) return;
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
    
        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

        if (!placementMap.HasTile(cellPos)) return;
        if (HasDecorationInCell(cellPos)) return;
        if (placedTowers.ContainsKey(cellPos)) return;
    
        Tower towerComponent = TowerSelectionUI.SelectedTowerPrefab.GetComponentInChildren<Tower>();

        if (towerComponent != null && towerComponent.data != null)
        {
            if (CoinManager.instance.coins >= towerComponent.data.towerPrice)
            {
                CoinManager.instance.UpdateCoins(-towerComponent.data.towerPrice);

                Vector3 worldCenter = placementMap.GetCellCenterWorld(cellPos);
                worldCenter.z = 0;
                
                GameObject newTower = Instantiate(TowerSelectionUI.SelectedTowerPrefab, worldCenter, Quaternion.identity);
                placedTowers.Add(cellPos, newTower);
            }
        }
    }

    void HandlePlacementHover()
    {
        if (GameManager.Instance != null && 
            (GameManager.Instance.CurrentState != GameState.Preparation && 
             GameManager.Instance.CurrentState != GameState.WaveRunning || 
             GameManager.Instance.IsPaused))
        {
            if (ghostInstance != null) Destroy(ghostInstance);
            return;
        }

        if (TowerSelectionUI.SelectedTowerPrefab == null)
        {
            if (ghostInstance != null)
                Destroy(ghostInstance);
            currentGhostPrefab = null;
            return;
        }

        if (ghostInstance == null || currentGhostPrefab != TowerSelectionUI.SelectedTowerPrefab)
        {
            if (ghostInstance != null)
                Destroy(ghostInstance);

            currentGhostPrefab = TowerSelectionUI.SelectedTowerPrefab;
            ghostInstance = CreateVisualClone(currentGhostPrefab);
            ghostInstance.AddComponent<GhostTower>();
        }
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

        Vector3 worldCenter = placementMap.GetCellCenterWorld(cellPos);
        worldCenter.z = 0;

        ghostInstance.transform.position = worldCenter + new Vector3(0,0);
        
        bool canAfford = true;
        Tower towerComponent = TowerSelectionUI.SelectedTowerPrefab.GetComponentInChildren<Tower>();
        if (towerComponent != null && towerComponent.data != null)
        {
            canAfford = CoinManager.instance.coins >= towerComponent.data.towerPrice;
        }

        bool valid = placementMap.HasTile(cellPos) && !placedTowers.ContainsKey(cellPos) && canAfford;
        if (HasDecorationInCell(cellPos)) valid = false;
        
        if (ghostInstance.TryGetComponent<GhostTower>(out var ghostScript))
        {
            ghostScript.SetValid(valid);
            
            if (towerComponent != null && towerComponent.data != null)
            {
                ghostScript.SetRadius(towerComponent.data.range);
            }
        }
    }
} 
