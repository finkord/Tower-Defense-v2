using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public EnemyData data; 
    
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public int currentReward;
    
    public Transform[] waypoints;
    public int currentWaypoint = 0;

    [Header("Movement Settings")]
    public float turnSpeed = 360f; 
    public float rotationOffset = 0f; 

    [Header("UI Settings")]
    public Slider healthSlider;
    public Transform canvasTransform; 

    public void Init(float healthMultiplier = 1f, float speedMultiplier = 1f, float rewardMultiplier = 1f)
    {
        if (data != null)
        {
            currentHealth = Mathf.RoundToInt(data.health * healthMultiplier);
            currentSpeed = data.speed * speedMultiplier;
            currentReward = Mathf.RoundToInt(data.reward * rewardMultiplier);
            currentWaypoint = 0; 
            
            StopCoroutine(nameof(SlowRoutine)); 
            
            if (healthSlider != null)
            {
                healthSlider.maxValue = currentHealth;
                healthSlider.value = currentHealth;
            }
        }
    }
    
    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        Transform target = waypoints[currentWaypoint];
        Vector3 dir = (target.position - transform.position).normalized;
        
        transform.position = Vector3.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) <= 0.001f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                HealthManager.Instance.UpdateHealth(-1); 
                gameObject.SetActive(false); 
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            gameObject.SetActive(false); 
        }
    }

    void LateUpdate()
    {
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