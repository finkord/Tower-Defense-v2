using UnityEngine;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private int baseHealth = 20;

    public void ReduceBaseHealth(int damage)
    {
        baseHealth -= damage;
        Debug.Log("Base Health: " + baseHealth);

        if (baseHealth <= 0)
        {
            Debug.Log("Game Over");
            // Add SceneManager.LoadScene here for restart
        }
    }
}