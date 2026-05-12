using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData data; // Reference to ScriptableObject
    public Transform firePoint;
    public Transform turretPart;
    
    private float fireCooldown = 0f;

    void Update()
    {
        if (data == null) return;

        fireCooldown -= Time.deltaTime;
        Enemy target = FindBestTarget();

        if (target != null)
        {
            RotateTowardsTarget(target.transform);

            if (fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1f / data.fireRate;
            }
        }
    }

    void RotateTowardsTarget(Transform targetTransform)
    {
        if (turretPart == null) return;
        
        Vector3 direction = targetTransform.position - turretPart.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
        turretPart.rotation = Quaternion.Lerp(turretPart.rotation, targetRotation, data.rotationSpeed * Time.deltaTime);
    }

    Enemy FindBestTarget()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy best = null;
        float bestProgress = -1f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);

            if (dist <= data.range)
            {
                if (e.currentWaypoint > bestProgress)
                {
                    bestProgress = e.currentWaypoint;
                    best = e;
                }
            }
        }
        return best;
    }

    void Shoot(Enemy target)
    {
        if (data.projectilePrefab != null && firePoint != null)
        {
            GameObject p = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile pr = p.GetComponent<Projectile>();
        
            if (pr != null)
            {
                pr.target = target.transform;
                // Pass the entire ScriptableObject reference
                pr.data = this.data; 
            }
        }
    }
}