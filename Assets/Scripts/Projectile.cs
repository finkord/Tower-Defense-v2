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
        if (data == null) return;

        if (data.isAoE)
        {
            Debug.Log($"AoE Explosion! Radius: {data.explosionRadius}");
        
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
            if (hitPSPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitPSPrefab, e.transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySFX(hitSFX);
            }
            CoinManager.instance.UpdateCoins(e.currentReward);
        }
    }
}