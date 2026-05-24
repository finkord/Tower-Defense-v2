using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed;
    public int health;
    public int reward;
    public int attackCost = 50;
    
    [Header("Special Features")]
    public bool immuneToSlow; 
}