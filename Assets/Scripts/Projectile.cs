using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public Transform target;
    
    public GameObject hitPSPrefab;
    public AudioClip hitSFX;

    public GameObject deathPSPrefab;
    public AudioClip deathSFX;
    
    [HideInInspector] public TowerData data; 

    private static Dictionary<GameObject, List<GameObject>> particlePools = new Dictionary<GameObject, List<GameObject>>();

    private GameObject SpawnParticle(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;

        if (!particlePools.ContainsKey(prefab))
        {
            particlePools[prefab] = new List<GameObject>();
        }

        particlePools[prefab].RemoveAll(item => item == null);

        foreach (var obj in particlePools[prefab])
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.SetActive(true);
                return obj;
            }
        }

        GameObject newObj = Instantiate(prefab, position, Quaternion.identity);
        particlePools[prefab].Add(newObj);
        return newObj;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        Enemy enemy = target.GetComponent<Enemy>();
        Vector3 targetVelocity = Vector3.zero;
        if (enemy != null && enemy.waypoints != null && enemy.currentWaypoint < enemy.waypoints.Length)
        {
            Vector3 targetDir = (enemy.waypoints[enemy.currentWaypoint].position - enemy.transform.position).normalized;
            targetVelocity = targetDir * enemy.currentSpeed;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        float moveDistance = speed * Time.deltaTime;

        if (dist <= moveDistance || dist < 0.15f)
        {
            ApplyImpact();
            gameObject.SetActive(false);
            return;
        }

        float timeToReach = dist / speed;
        Vector3 predictedPos = target.position + targetVelocity * timeToReach;
        
        Vector3 dir = (predictedPos - transform.position).normalized;
        transform.position += dir * moveDistance;
        
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    void ApplyImpact()
    {
        if (hitPSPrefab != null)
        {
            SpawnParticle(hitPSPrefab, transform.position);
        }
        if (hitSFX != null)
        {
            AudioManager.Instance.PlaySFX(hitSFX);
        }

        if (data == null) return;

        if (data.isAoE)
        {
            // Debug.Log($"AoE Explosion! Radius: {data.explosionRadius}");
        
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.explosionRadius);
            
            foreach (var col in hitEnemies)
            {
                Enemy e = col.GetComponent<Enemy>();
                if (e != null) ProcessEnemy(e);
            }
        }
        else
        {
            Enemy e = target.GetComponent<Enemy>();
            if (e != null) ProcessEnemy(e);
        }
    }

    void ProcessEnemy(Enemy e)
    {
        bool wasAlive = e.currentHealth > 0;
        
        if (data.isSlowing && wasAlive)
        {
            e.ApplySlow(data.slowFactor, data.slowDuration);
        }

        e.TakeDamage(data.damage);

        if (wasAlive && e.currentHealth <= 0)
        {
            if (deathPSPrefab != null)
            {
                SpawnParticle(deathPSPrefab, e.transform.position);
            }
            if (deathSFX != null)
            {
                AudioManager.Instance.PlaySFX(deathSFX);
            }
            CoinManager.instance.UpdateCoins(e.currentReward);
        }
    }
}