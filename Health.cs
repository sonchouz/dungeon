using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDeath;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public event System.Action<float> OnHealthChanged;
    private int currentHealth;
    private bool isDead;
    private Animator animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        onDamaged?.Invoke();
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        if (animator != null) animator.SetTrigger("Die");
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
        Destroy(gameObject);
        onDeath?.Invoke();
    }
}
