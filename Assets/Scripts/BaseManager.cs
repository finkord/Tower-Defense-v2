using UnityEngine;
using UnityEngine.UI;

public class BaseManager : MonoBehaviour
{
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private Image healthBarFill;
    private float _currentHealth;

    void Start()
    {
        _currentHealth = maxHealth;
        UpdateUI();
    }

    public void ReduceBaseHealth(float damage)
    {
        _currentHealth -= damage;
        UpdateUI();

        if (_currentHealth <= 0)
        {
            Debug.Log("Game Over");
            // Logic for Game Over scene
        }
    }

    private void UpdateUI()
    {
        if (healthBarFill != null)
        {
            // Healthbar update logic
            healthBarFill.fillAmount = _currentHealth / maxHealth;
        }
    }
}