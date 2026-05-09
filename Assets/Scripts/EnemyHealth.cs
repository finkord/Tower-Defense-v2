using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBarFill;
    private float _currentHealth;

    void Start()
    {
        _currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = _currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        // Add logic for gold/exp here later
        Destroy(gameObject);
    }
}