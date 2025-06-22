using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private int patrolPointCount = 4;
    [SerializeField] private float patrolPadding = 1f;
    [SerializeField] private float waitAtPoint = 1.2f;

    [Header("Detection / Attack")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1.1f;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Links")]
    [SerializeField] private AttackController attackController;

    private enum State { Patrol, Chase, Attack }

    private NavMeshAgent agent;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform player;

    private readonly List<Vector3> patrolPts = new();
    private int ptIndex;
    private float waitTimer;
    private float atkTimer;

    private State state = State.Patrol;
    private Collider2D currentRoomBounds; // Динамическая ссылка на текущую комнату

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.updatePosition = false;

        player = GameObject.FindWithTag("Character")?.transform;
        if (attackController == null)
            attackController = GetComponent<AttackController>();
    }

    private void Start()
    {
        UpdateCurrentRoom(); // Инициализация текущей комнаты
        GeneratePatrolPoints();
        GoNextPoint();
        waitTimer = waitAtPoint;
    }

    private void LateUpdate()
    {
        var p = agent.nextPosition;
        transform.position = new Vector3(p.x, p.y, transform.position.z);
        UpdateCurrentRoom(); // Обновление текущей комнаты в реальном времени
    }

    private void Update()
    {
        if (player == null || currentRoomBounds == null)
            return;

        var dist = Vector2.Distance(transform.position, player.position);
        var inCurrentRoom = currentRoomBounds.bounds.Contains(player.position);

        switch (state)
        {
            case State.Patrol:
                PatrolTick();
                if (inCurrentRoom && dist <= detectionRadius) state = State.Chase;
                break;
            case State.Chase:
                ChaseTick(dist, inCurrentRoom);
                break;
            case State.Attack:
                AttackTick(dist, inCurrentRoom);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateAnim();
    }

    private void PatrolTick()
    {
        if (agent.pathPending)
            return;

        if (!(agent.remainingDistance <= agent.stoppingDistance))
            return;

        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            GoNextPoint();
            waitTimer = waitAtPoint;
        }
    }

    private void GeneratePatrolPoints()
    {
        patrolPts.Clear();

        if (currentRoomBounds != null)
        {
            for (var i = 0; i < patrolPointCount; i++)
                patrolPts.Add(NavMeshUtils.GetRandomPointInRoom(currentRoomBounds, patrolPadding));
        }
        else
        {
            Debug.LogWarning("[EnemyAI] Нет текущей комнаты для генерации патрульных точек!");
        }
    }

    private void GoNextPoint()
    {
        if (patrolPts.Count == 0)
            return;

        agent.isStopped = false;
        agent.SetDestination(patrolPts[ptIndex]);
        ptIndex = (ptIndex + 1) % patrolPts.Count;
    }
    private void ChaseTick(float dist, bool inRoom)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (!inRoom || dist > detectionRadius * 1.3f)
            state = State.Patrol;
        else if (dist <= attackRadius * 0.98f)
            BeginAttack();
    }

    private void BeginAttack()
    {
        state = State.Attack;
        atkTimer = attackCooldown;
        agent.isStopped = true;

        Vector2 dir = (player.position - transform.position).normalized;
        attackController.Attack(dir);
    }

    private void AttackTick(float dist, bool inRoom)
    {
        atkTimer -= Time.deltaTime;

        if (atkTimer > 0f)
            return;

        if (inRoom && dist <= attackRadius)
        {
            BeginAttack();
            return;
        }

        state = (inRoom && dist <= detectionRadius) ? State.Chase : State.Patrol;
        agent.isStopped = false;
    }

    private void UpdateAnim()
    {
        Vector2 v = agent.velocity;
        var spd = v.magnitude;

        if (spd > 0.01f)
            sr.flipX = v.x < 0;

        anim.SetFloat(MoveX, v.x);
        anim.SetFloat(MoveY, v.y);
        anim.SetFloat(Speed, spd);
    }

    private void UpdateCurrentRoom()
    {
        // Поиск ближайшей комнаты, содержащей врага
        RoomInstance[] rooms = FindObjectsOfType<RoomInstance>();
        if (rooms.Length == 0)
        {
            currentRoomBounds = null;
            return;
        }

        Collider2D closestRoom = null;
        float minDistance = float.MaxValue;

        foreach (var room in rooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            if (roomCollider != null && roomCollider.bounds.Contains(transform.position))
            {
                float distance = Vector2.Distance(transform.position, roomCollider.bounds.center);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRoom = roomCollider;
                }
            }
        }

        currentRoomBounds = closestRoom;

        // Обновление патрульных точек, если комната изменилась
        if (currentRoomBounds != null && (patrolPts.Count == 0 || !patrolPts.Any(pt => currentRoomBounds.bounds.Contains(pt))))
        {
            GeneratePatrolPoints();
            GoNextPoint();
        }
    }
}
