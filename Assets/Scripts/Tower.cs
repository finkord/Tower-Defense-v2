using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 3f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        Enemy target = FindBestTarget();

        if (target != null)
        {
            RotateTowardsTarget(target.transform);

            if (fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1f / fireRate;
            }
        }
    }
    
    public float rotationSpeed = 10f;

    void RotateTowardsTarget(Transform targetTransform)
    {
        Vector3 direction = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // void RotateTowardsTarget(Transform targetTransform)
    // {
    //     Vector3 direction = targetTransform.position - transform.position;
    //     
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //     
    //     float rotationOffset = -90f; 
    //
    //     transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    // }

    Enemy FindBestTarget()
    {
        // Виправлено: FindObjectsOfType викликається без префікса GameObject
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        Enemy best = null;
        float bestProgress = -1f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);

            if (dist <= range)
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
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Projectile pr = p.GetComponent<Projectile>();
            pr.target = target.transform;
        }
    }
}