using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public EnemyData data; 
    
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public int currentHealth;
    
    public Transform[] waypoints;
    public int currentWaypoint = 0;

    [Header("Movement Settings")]
    public float turnSpeed = 360f; // Rotation speed in degrees per second
    public float rotationOffset = 0f; // Adjust if the sprite does not face right natively

    [Header("UI Settings")]
    public Slider healthSlider;
    public Transform canvasTransform; // Assign Canvas here to prevent it from spinning

    void Start()
    {
        if (data != null)
        {
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
        
        // Move towards waypoint
        transform.position += dir * currentSpeed * Time.deltaTime;

        // Rotate enemy towards movement direction
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                HealthManager.Instance.UpdateHealth(-1); 
                Destroy(gameObject);
            }
        }
    }

    void LateUpdate()
    {
        // Lock Canvas rotation so UI elements remain upright
        if (canvasTransform != null)
        {
            canvasTransform.rotation = Quaternion.identity;
        }
    }

    public void ApplySlow(float factor, float duration)
    {
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