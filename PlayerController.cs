using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir;
    private PlayerInputActions inputActions;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public float speed = 5f;
    private bool isAttacking = false;
    private float attackCooldown = 0.5f;
    private float attackTimer = 0f;

    [SerializeField] private AttackHitbox hitboxUp;
    [SerializeField] private AttackHitbox hitboxDown;
    [SerializeField] private AttackHitbox hitboxLeft;
    [SerializeField] private AttackHitbox hitboxRight;
    [SerializeField] private AttackHitboxController hitboxController;
    private AttackHitbox currentActiveHitbox;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        inputActions = new PlayerInputActions();
        inputActions.Gameplay.Attack.performed += ctx => OnAttack();
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    private void Update()
    {
        moveInput = inputActions.Gameplay.Movement.ReadValue<Vector2>();
        spriteRenderer.flipX = moveInput.x < -0.01f;

        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;

        animator.SetFloat("MoveX", lastMoveDir.x);
        animator.SetFloat("MoveY", lastMoveDir.y);
        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                isAttacking = false;
        }
    }

    private void FixedUpdate()
    {
        var targetPosition = rb.position + moveInput * (speed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }

    private void OnAttack()
    {
        if (isAttacking) return;
        isAttacking = true;
        attackTimer = attackCooldown;
        if (lastMoveDir.y > 0.5f)
            animator.SetTrigger("AttackUp");
        else if (Mathf.Abs(lastMoveDir.x) > 0.5f)
            animator.SetTrigger("AttackSide");
        else
            animator.SetTrigger("AttackDown");
    }

    public void EnableAttackHitbox()
    {
        SelectAttackHitbox();
        currentActiveHitbox?.EnableHitbox();
    }

    public void DisableAttackHitbox()
    {
        currentActiveHitbox?.DisableHitbox();
    }

    private void SelectAttackHitbox()
    {
        if (lastMoveDir.y > 0.5f)
            currentActiveHitbox = hitboxUp;
        else if (lastMoveDir.y < -0.5f)
            currentActiveHitbox = hitboxDown;
        else if (lastMoveDir.x < -0.5f)
            currentActiveHitbox = hitboxLeft;
        else
            currentActiveHitbox = hitboxRight;
    }
}