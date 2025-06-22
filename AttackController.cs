using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AttackController : MonoBehaviour
{
    [SerializeField] private AttackHitboxController hitboxController;
    private Animator _anim;
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");
    private static readonly int AttackHash = Animator.StringToHash("AttackTrigger");
    private Vector2 _lastAttackDir;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void Attack(Vector2 attackDir)
    {
        _lastAttackDir = attackDir.normalized;
        _anim.SetFloat(MoveXHash, _lastAttackDir.x);
        _anim.SetFloat(MoveYHash, _lastAttackDir.y);
        _anim.SetTrigger(AttackHash);
    }

    public void EnableHitbox() => hitboxController.EnableHitbox(_lastAttackDir);
    public void DisableHitbox() => hitboxController.DisableHitbox();
}
