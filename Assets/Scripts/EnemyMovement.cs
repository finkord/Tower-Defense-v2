using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    private Transform[] _waypoints;
    private int _currentWaypointIndex = 0;

    void Start()
    {
        // Get all children of the Waypoints container
        GameObject waypointContainer = GameObject.Find("Waypoints");
        if (waypointContainer != null)
        {
            _waypoints = new Transform[waypointContainer.transform.childCount];
            for (int i = 0; i < waypointContainer.transform.childCount; i++)
            {
                _waypoints[i] = waypointContainer.transform.GetChild(i);
            }
        }
        
        // Start at the first waypoint position
        if (_waypoints.Length > 0)
        {
            transform.position = _waypoints[0].position;
        }
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        if (_currentWaypointIndex >= _waypoints.Length) return;

        // Move towards the current target waypoint
        Transform target = _waypoints[_currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Check if we reached the waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            _currentWaypointIndex++;
        }

        // Optional: Destroy enemy when reached the end
        if (_currentWaypointIndex >= _waypoints.Length)
        {
            // Find base and apply damage
            BaseManager baseManager = Object.FindFirstObjectByType<BaseManager>();
            if (baseManager != null)
            {
                baseManager.ReduceBaseHealth(1);
            }
            Destroy(gameObject);
        }
    }
}