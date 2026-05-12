using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public EnemyData data; // Reference to the ScriptableObject
    
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public int currentHealth;
    
    public Transform[] waypoints;
    public int currentWaypoint = 0;

    [Header("UI Settings")]
    public Slider healthSlider;

    void Start()
    {
        if (data != null)
        {
            // Initialize stats from ScriptableObject data
            currentHealth = data.health;
            currentSpeed = data.speed;
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = data.health;
                healthSlider.value = currentHealth;
            }
        }
    }
    
    void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (waypoints == null || waypoints.Length == 0) return;
        
        Transform target = waypoints[currentWaypoint];
        Vector3 dir = (target.position - transform.position).normalized;
        
        // Move using the current speed (which might be modified by slow)
        transform.position += dir * currentSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                HealthManager.Instance.UpdateHealth(-1); //
                Destroy(gameObject);
            }
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        // Check if the enemy type ignores freezing
        if (data != null && data.immuneToSlow) return;

        StopCoroutine(nameof(SlowRoutine));
        StartCoroutine(SlowRoutine(factor, duration));
    }

    private IEnumerator SlowRoutine(float factor, float duration)
    {
        float originalSpeed = data.speed;
        currentSpeed = originalSpeed * factor;

        yield return new WaitForSeconds(duration);

        currentSpeed = originalSpeed;
    }
}