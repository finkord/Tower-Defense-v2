using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public Transform target;
    
    public GameObject hitPSPrefab;

    public AudioClip hitSFX;
    
    [HideInInspector] public TowerData data; 

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        
        // Simple rotation logic
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        if (Vector2.Distance(transform.position, target.position) < 0.15f)
        {
            ApplyImpact();
            Destroy(gameObject);
        }
    }

    void ApplyImpact()
    {
        if (data == null) return;

        if (data.isAoE)
        {
            // Draw debug sphere in console/editor
            Debug.Log($"AoE Explosion! Radius: {data.explosionRadius}");
        
            // Use a LayerMask if your enemies are on a specific layer
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, data.explosionRadius);
            
            foreach (var col in hitEnemies)
            {
                Enemy e = col.GetComponent<Enemy>();
                if (e != null) ProcessEnemy(e);
            }
        }
        else
        {
            // Handle single target for Archer/Freezer
            Enemy e = target.GetComponent<Enemy>();
            if (e != null) ProcessEnemy(e);
        }
    }

    void ProcessEnemy(Enemy e)
    {
        e.currentHealth -= data.damage;
    
        if (data.isSlowing)
        {
            e.ApplySlow(data.slowFactor, data.slowDuration);
        }

        if (e.currentHealth <= 0)
        {
            if (hitPSPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitPSPrefab, e.transform.position, Quaternion.identity);
                // Destroy(hitEffect, 2f); 
                AudioManager.Instance.PlaySFX(hitSFX);
            }
            
            CoinManager.instance.UpdateCoins(e.data.reward);
            Destroy(e.gameObject);
        }
    }
}