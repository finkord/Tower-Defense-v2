using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 3;
    public Transform[] waypoints;
    public int currentWaypoint = 0;

    [Header("UI Settings")]
    public Slider healthSlider;
    private int maxHealth;

    void Start()
    {
        // Initialize health values
        maxHealth = health;
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }
    
    void Update()
    {
        // Sync health bar with current health
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (waypoints == null || waypoints.Length == 0) return;
        
        Transform target = waypoints[currentWaypoint];
        Vector3 dir = (target.position - transform.position).normalized;
        
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                Destroy(gameObject);
            }
        }
    }
}