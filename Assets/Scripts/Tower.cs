using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public TowerData data; 
    public Transform[] firePoints;
    private int currentFirePointIndex = 0;
    public Transform turretPart;
    
    private float fireCooldown = 0f;
    
    public GameObject cloudPSPrefab;
    public AudioClip shootSFX;

    private static Dictionary<GameObject, List<GameObject>> projectilePools = new Dictionary<GameObject, List<GameObject>>();

    void Start()
    {
        
        if (cloudPSPrefab != null)
        {
            GameObject cloudEffect = Instantiate(cloudPSPrefab, transform.position, Quaternion.identity);
        }
    }
    
    void Update()
    {
        if (data == null) return;

        fireCooldown -= Time.deltaTime;
        Enemy target = FindBestTarget();

        if (target != null)
        {
            Vector3 predictedPos = target.transform.position;
            float projectileSpeed = 10f;
            if (data.projectilePrefab != null)
            {
                projectileSpeed = data.projectilePrefab.GetComponent<Projectile>().speed;
            }
            
            Vector3 aimOrigin = firePoints != null && firePoints.Length > 0 && firePoints[0] != null ? firePoints[0].position : transform.position;
            float dist = Vector2.Distance(aimOrigin, target.transform.position);
            float timeToReach = dist / projectileSpeed;
            
            if (target.waypoints != null && target.currentWaypoint < target.waypoints.Length)
            {
                Vector3 targetDir = (target.waypoints[target.currentWaypoint].position - target.transform.position).normalized;
                Vector3 targetVelocity = targetDir * target.currentSpeed;
                predictedPos = target.transform.position + targetVelocity * timeToReach;
            }

            RotateTowardsPosition(predictedPos);

            if (fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1f / data.fireRate;
            }
        }
    }

    void RotateTowardsPosition(Vector3 targetPos)
    {
        if (turretPart == null) return;
        
        Vector3 direction = targetPos - turretPart.position;
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
        if (data.projectilePrefab != null)
        {
            Transform currentFirePoint = transform;
            if (firePoints != null && firePoints.Length > 0 && firePoints[0] != null)
            {
                currentFirePoint = firePoints[currentFirePointIndex];

                currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
                while (firePoints[currentFirePointIndex] == null && currentFirePointIndex != 0)
                {
                     currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
                }
            }

            if (!projectilePools.ContainsKey(data.projectilePrefab))
            {
                projectilePools[data.projectilePrefab] = new List<GameObject>();
            }

            projectilePools[data.projectilePrefab].RemoveAll(item => item == null);

            GameObject p = null;
            foreach (var obj in projectilePools[data.projectilePrefab])
            {
                if (!obj.activeInHierarchy)
                {
                    p = obj;
                    break;
                }
            }

            if (p == null)
            {
                p = Instantiate(data.projectilePrefab, currentFirePoint.position, Quaternion.identity);
                projectilePools[data.projectilePrefab].Add(p);
            }
            else
            {
                p.transform.position = currentFirePoint.position;
                p.transform.rotation = Quaternion.identity;
                p.SetActive(true);
            }

            Projectile pr = p.GetComponent<Projectile>();
        
            if (pr != null)
            {
                pr.target = target.transform;
                pr.data = this.data; 
            }
            
            if (shootSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(shootSFX);
            }
        }
    }
}