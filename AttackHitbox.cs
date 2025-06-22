using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 1;
    public string[] targetTags = { "Enemy" };
    public bool active;

    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

    public void EnableHitbox()
    {
        active = true;
        alreadyHit.Clear();
    }

    public void DisableHitbox()
    {
        active = false;
    }

    private void DealDamage(Collider2D other)
    {
        if (alreadyHit.Contains(other)) return;
        foreach (var tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                var health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    alreadyHit.Add(other);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (active) DealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (active) DealDamage(other);
    }
}