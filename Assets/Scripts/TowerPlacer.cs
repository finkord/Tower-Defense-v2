using UnityEngine;

using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TowerPlacer : MonoBehaviour
{
    public Tilemap placementMap;
    public Tilemap nonPlacementMap;

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
    
    void Update()
    {
        HandleTowerDeletion();
        HandlePlacementClick();
        HandlePlacementHover();
    }

    void HandleTowerDeletion()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            if (TowerSelectionUI.SelectedTowerPrefab != null)
            {
                // Cancel placement
                TowerSelectionUI.SelectedTowerPrefab = null;
                return;
            }

            // Otherwise try to delete placed tower
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

            if (placedTowers.TryGetValue(cellPos, out GameObject towerToDelete))
            {
                Tower towerComponent = towerToDelete.GetComponentInChildren<Tower>();
                if (towerComponent != null && towerComponent.data != null)
                {
                    // Refund 50% of the price
                    CoinManager.instance.UpdateCoins(towerComponent.data.towerPrice / 2);
                }
                
                Destroy(towerToDelete);
                placedTowers.Remove(cellPos);
            }
        }
    }

    void HandlePlacementClick()
    {
        if(!Input.GetMouseButtonDown(0)) return;
        if (TowerSelectionUI.SelectedTowerPrefab == null) return;
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
    
        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

        if (!placementMap.HasTile(cellPos)) return;
        if (placedTowers.ContainsKey(cellPos)) return;
    
        // Get the Tower component to access its data
        Tower towerComponent = TowerSelectionUI.SelectedTowerPrefab.GetComponentInChildren<Tower>();

        if (towerComponent != null && towerComponent.data != null)
        {
            if (CoinManager.instance.coins >= towerComponent.data.towerPrice)
            {
                CoinManager.instance.UpdateCoins(-towerComponent.data.towerPrice);
                
                // Calculate precise center based on current cell position
                Vector3 worldCenter = placementMap.GetCellCenterWorld(cellPos);
                worldCenter.z = 0;
                
                GameObject newTower = Instantiate(TowerSelectionUI.SelectedTowerPrefab, worldCenter, Quaternion.identity);
                placedTowers.Add(cellPos, newTower);
            }
            // If they can't afford it, it does nothing (no placement, no deselection)
        }
    }

    void HandlePlacementHover()
    {
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
