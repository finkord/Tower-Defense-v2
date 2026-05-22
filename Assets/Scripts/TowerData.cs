using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerData", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public float range;
    public float fireRate;
    public float rotationSpeed;
    public int towerPrice;
    public int damage;
    public GameObject projectilePrefab;

    [Header("Special Abilities")]
    public bool isAoE;
    public float explosionRadius;
    public bool isSlowing;
    public float slowFactor; 
    public float slowDuration;
}