using UnityEngine;

public class HealthBinder : MonoBehaviour
{
    public MonoBehaviour uiHandler;

    private void Start()
    {
        var health = GetComponent<Health>();
        if (health == null) return;
        var handler = uiHandler as IHealthUIHandler ?? GetComponentInChildren<IHealthUIHandler>();
        if (handler == null)
        {
            Debug.LogWarning("[HealthBinder] UI Handler not found!");
            return;
        }
        health.OnHealthChanged += handler.UpdateHealthUI;
        handler.SetupUI(health.GetMaxHealth());
        handler.UpdateHealthUI((float)health.GetCurrentHealth() / health.GetMaxHealth());
    }
}