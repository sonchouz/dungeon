using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour, IHealthUIHandler
{
    public Image foregroundImage;
    public Transform target;
    public Vector3 offset;

    public void SetupUI(float maxHealth) { }
    public void UpdateHealthUI(float normalizedValue)
    {
        foregroundImage.fillAmount = normalizedValue;
        if (normalizedValue <= 0f) Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (target != null) transform.position = target.position + offset;
    }
}