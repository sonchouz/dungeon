using UnityEngine;

public class AttackHitboxController : MonoBehaviour
{
    [SerializeField] private AttackHitbox hitboxUp;
    [SerializeField] private AttackHitbox hitboxDown;
    [SerializeField] private AttackHitbox hitboxLeft;
    [SerializeField] private AttackHitbox hitboxRight;
    private AttackHitbox _current;

    public void EnableHitbox(Vector2 attackDir)
    {
        DisableHitbox();
        if (attackDir.y > 0.5f) _current = hitboxUp;
        else if (attackDir.y < -0.5f) _current = hitboxDown;
        else if (attackDir.x < -0.5f) _current = hitboxLeft;
        else _current = hitboxRight;
        _current?.EnableHitbox();
    }

    public void DisableHitbox()
    {
        _current?.DisableHitbox();
        _current = null;
    }
}
