using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-100)] // Instance must exist before subscribers run Awake or OnEnable
public class AIGameManager : MonoBehaviour
{
    private const int MaxSpawnAttempts = 20;

    public static AIGameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AIAgent agentPrefab;
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float friendlySpeed = 2f;
    [SerializeField, Min(0f)] private float enemySpeed = 2f;
    [SerializeField, Min(0f)] private float wanderSpeed = 1f;
    [SerializeField, Min(0.01f)] private float wanderTurnInterval = 2f;
    [SerializeField, Min(0f)] private float arrivalRadius = 0.75f;

    [Header("Enemy Behavior")]
    [SerializeField, Min(0f)] private float detectionRadius = 3f;
    [SerializeField, Min(0f)] private float attackRange = 0.5f;
    [SerializeField, Min(0f)] private float attackDuration = 2f;
    [SerializeField, Min(0f)] private float seenDestroyDuration = 2f;

    [Header("Spawning")]
    [SerializeField, Min(0)] private int initialFriendlies = 8;
    [SerializeField, Min(0.01f)] private float enemySpawnInterval = 1.5f;
    [SerializeField, Min(0)] private int maxEnemies = 20;
    [SerializeField, Min(0f)] private float minSpawnDistanceFromTarget = 2f;

    [Header("Appearance")]
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;

    private readonly List<AIAgent> friendlies = new List<AIAgent>();
    private readonly List<AIAgent> enemies = new List<AIAgent>();
    private Camera mainCamera;
    private float spawnTimer;
    private Vector2 seenZonePosition;
    private float seenZoneRadius;

    public float FriendlySpeed => friendlySpeed;
    public float EnemySpeed => enemySpeed;
    public float WanderSpeed => wanderSpeed;
    public float WanderTurnInterval => wanderTurnInterval;
    public float ArrivalRadius => arrivalRadius;
    public float DetectionRadius => detectionRadius;
    public float AttackRange => attackRange;
    public float AttackDuration => attackDuration;
    public float SeenDestroyDuration => seenDestroyDuration;
    public Vector2 TargetPosition => target.position;
    public Rect BoardRect => GetCameraRect();

    public int StartingFriendlyCount => initialFriendlies;
    public bool HasFriendlyDied { get; private set; }
    public int FriendliesReachedTarget { get; private set; }

    public event Action<int> FriendliesSpawned; // Passes the starting count
    public event Action FriendlyDied;
    public event Action<int> FriendlyReachedTarget; // Passes the total that have arrived so far

    // A radius of 0 means nothing is seen
    public void SetSeenZone(Vector2 position, float radius)
    {
        seenZonePosition = position;
        seenZoneRadius = Mathf.Max(0f, radius);
    }

    public bool IsInSeenZone(Vector2 position)
    {
        return seenZoneRadius > 0f && Vector2.Distance(position, seenZonePosition) <= seenZoneRadius;
    }

    public void KillFriendly(AIAgent friendly)
    {
        HasFriendlyDied = true;
        Despawn(friendly);
        FriendlyDied?.Invoke();
    }

    public void ArriveFriendly(AIAgent friendly)
    {
        FriendliesReachedTarget++;
        Despawn(friendly);
        FriendlyReachedTarget?.Invoke(FriendliesReachedTarget);
    }

    public void Despawn(AIAgent agent)
    {
        friendlies.Remove(agent);
        enemies.Remove(agent);
        Destroy(agent.gameObject);
    }

    public AIAgent FindNearestFriendly(Vector2 position, float radius)
    {
        AIAgent nearest = null;
        float nearestDistance = radius;

        foreach (AIAgent friendly in friendlies)
        {
            float distance = Vector2.Distance(position, friendly.transform.position);

            if (distance <= nearestDistance)
            {
                nearest = friendly;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        for (int i = 0; i < initialFriendlies; i++)
        {
            friendlies.Add(SpawnAgent(AIRole.Friendly, friendlyColor));
        }

        FriendliesSpawned?.Invoke(initialFriendlies);
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < enemySpawnInterval)
        {
            return;
        }

        spawnTimer = 0f;

        if (enemies.Count < maxEnemies)
        {
            enemies.Add(SpawnAgent(AIRole.Enemy, enemyColor));
        }
    }

    private AIAgent SpawnAgent(AIRole role, Color color)
    {
        AIAgent agent = Instantiate(agentPrefab, GetSpawnPosition(), Quaternion.identity, transform);
        agent.Initialize(role);
        agent.GetComponentInChildren<SpriteRenderer>().color = color;
        return agent;
    }

    private Vector2 GetSpawnPosition()
    {
        Rect area = GetCameraRect();
        Vector2 position = Vector2.zero;

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            position = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));

            if (Vector2.Distance(position, TargetPosition) >= minSpawnDistanceFromTarget)
            {
                break;
            }
        }

        return position;
    }

    private Rect GetCameraRect()
    {
        Camera activeCamera = mainCamera != null ? mainCamera : Camera.main; // Awake hasn't run when drawing gizmos in edit mode
        float halfHeight = activeCamera.orthographicSize;
        float halfWidth = halfHeight * activeCamera.aspect;
        Vector2 center = activeCamera.transform.position;
        return Rect.MinMaxRect(center.x - halfWidth, center.y - halfHeight, center.x + halfWidth, center.y + halfHeight);
    }

    private void OnDrawGizmos()
    {
        if (seenZoneRadius > 0f)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(seenZonePosition, seenZoneRadius);
        }

        if (Camera.main != null)
        {
            Rect area = GetCameraRect();
            Gizmos.color = Color.cyan; // The board: spawn area and wander bounds
            Gizmos.DrawWireCube(area.center, area.size);
        }

        if (target == null)
        {
            return;
        }

        Gizmos.color = friendlyColor; // Seen friendlies despawn inside this radius
        Gizmos.DrawWireSphere(target.position, arrivalRadius);
        Gizmos.color = Color.yellow; // Agents don't spawn inside this radius
        Gizmos.DrawWireSphere(target.position, minSpawnDistanceFromTarget);
    }
}
