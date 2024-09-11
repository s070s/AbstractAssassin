using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float projectileDamageTaken;
    [HideInInspector]
    public float currentHealth;

    public delegate void OnHealthChangedDelegate(float health);
    public event OnHealthChangedDelegate OnHealthChanged;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
                TakeDamage(projectileDamageTaken);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}