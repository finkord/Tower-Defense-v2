using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem; // Required for the new Input System

public class TileClickTracker : MonoBehaviour
{
    [SerializeField] private Tilemap gridTilemap;
    [SerializeField] private TileBase roadTile;

    private void Update()
    {
        // Verify mouse exists and check if left button was pressed this frame
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckTileAtMousePosition();
        }
    }

    private void CheckTileAtMousePosition()
    {
        // Read mouse position from the new Input System
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        
        // Convert screen position to world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        
        // Convert world position to grid cell position
        Vector3Int cellPosition = gridTilemap.WorldToCell(mouseWorldPos);
        
        // Get the tile at the calculated cell
        TileBase clickedTile = gridTilemap.GetTile(cellPosition);

        if (clickedTile == null)
        {
            Debug.Log("Clicked outside the map");
            return;
        }

        if (clickedTile == roadTile)
        {
            Debug.Log($"Road tile clicked at {cellPosition}. Cannot build tower here.");
        }
        else
        {
            Debug.Log($"Grass tile clicked at {cellPosition}. Ready to build!");
        }
    }
}