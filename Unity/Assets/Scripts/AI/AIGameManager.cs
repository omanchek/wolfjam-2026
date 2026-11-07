using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class FriendlyRarity
{
    public string name;
    [Min(0)] public int worth;

    public FriendlyRarity(string name, int worth)
    {
        this.name = name;
        this.worth = worth;
    }
}

[DefaultExecutionOrder(-100)] // Instance must exist before subscribers run Awake or OnEnable
public class AIGameManager : MonoBehaviour
{
    private const int MaxSpawnAttempts = 20;
    private const int DefaultFriendlyWorth = 1; // Used when the rarity list is empty
    private const float GatherRadiusToScreenHeight = 4f / 9f; // Matches half the screen's width across at 16:9

    public static AIGameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AIAgent agentPrefab;
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float friendlySpeed = 60f;
    [SerializeField, Min(0f)] private float enemySpeed = 60f;
    [SerializeField, Min(0f)] private float wanderSpeed = 30f;
    [SerializeField, Min(0.01f)] private float wanderTurnInterval = 2f;
    [SerializeField, Min(0f)] private float arrivalRadius = 22.5f;
    [SerializeField, Min(0f)] private float migrationDuration = 2f;

    [Header("Friendly Rarities")]
    [SerializeField] private List<FriendlyRarity> friendlyRarities = new List<FriendlyRarity>
    {
        new FriendlyRarity("Common", 1),
        new FriendlyRarity("Rare", 2),
        new FriendlyRarity("Legendary", 5)
    };

    [Header("Enemy Behavior")]
    [SerializeField, Min(0f)] private float detectionRadius = 90f;
    [SerializeField, Min(0f)] private float attackRange = 15f;
    [SerializeField, Min(0f)] private float attackDuration = 2f;
    [SerializeField, Min(0f)] private float seenDestroyDuration = 2f;

    [Header("Spawning")]
    [SerializeField, Min(0.01f)] private float enemySpawnInterval = 1.5f;
    [SerializeField, Min(0)] private int maxEnemies = 20;
    [SerializeField, Min(0f)] private float minSpawnDistanceFromTarget = 60f;
    [SerializeField, Min(0f)] private float minEnemySpawnDistanceFromFriendly = 90f;

    [Header("Appearance")]
    [SerializeField] private string friendlyAnimationState = "MothFlying";
    [SerializeField] private List<string> enemyAnimationStates = new List<string> { "BatFlying", "HawkFlying", "OwlFlying" };

    private readonly List<AIAgent> friendlies = new List<AIAgent>();
    private readonly List<AIAgent> enemies = new List<AIAgent>();
    private Camera mainCamera;
    private float spawnTimer;
    private bool isRoundActive;
    private Vector2 seenZonePosition;
    private float seenZoneRadius;
    [SerializeField] private LightManager lightManager;

    public float FriendlySpeed => friendlySpeed;
    public float EnemySpeed => enemySpeed;
    public float WanderSpeed => wanderSpeed;
    public float WanderTurnInterval => wanderTurnInterval;
    public float ArrivalRadius => arrivalRadius;
    public float MigrationDuration => migrationDuration;
    public float DetectionRadius => detectionRadius;
    public float AttackRange => attackRange;
    public float AttackDuration => attackDuration;
    public float SeenDestroyDuration => seenDestroyDuration;
    public Vector2 TargetPosition => target.position;
    public Rect BoardRect => GetCameraRect();
    public float GatherRadius => GetCameraRect().height * GatherRadiusToScreenHeight;
    public Vector2 SeenZonePosition => seenZonePosition;
    public float SeenZoneRadius => seenZoneRadius;
    public LightManager GetLightManager => lightManager;

    public int StartingFriendlyCount { get; private set; }
    public bool HasFriendlyDied { get; private set; }
    public int FriendliesReachedTarget { get; private set; }

    public event Action<int> FriendliesSpawned; // Passes the starting count
    public event Action FriendlyDied;
    public event Action<int> FriendlyReachedTarget; // Passes the total that have arrived so far
    public event Action<int> FriendlyWorthCollected; // Passes what the arriving friendly was worth
    public event Action AllFriendliesReachedTarget; // Only fires if no friendly died this round
    public event Action AllFriendliesGone; // Fires when the last friendly has either arrived or died
    public event Action EnemyDestroyed;

    // Clears the board, resets the round stats, and spawns on the camera's current view
    public void StartRound(int friendlyCount)
    {
        DestroyAll(friendlies);
        DestroyAll(enemies);

        StartingFriendlyCount = Mathf.Max(0, friendlyCount);
        HasFriendlyDied = false;
        FriendliesReachedTarget = 0;
        spawnTimer = 0f;
        isRoundActive = true;

        for (int i = 0; i < StartingFriendlyCount; i++)
        {
            AIAgent friendly = Instantiate(agentPrefab, GetFriendlySpawnPosition(), Quaternion.identity, transform);
            friendly.Initialize(AIRole.Friendly, friendlyAnimationState, GetRandomRarity());
            friendlies.Add(friendly);
        }

        FriendliesSpawned?.Invoke(StartingFriendlyCount);
    }

    // Stops enemy spawning. Agents already on the board keep acting until the next StartRound.
    public void StopRound()
    {
        isRoundActive = false;
    }

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

    public bool IsInGatherZone(Vector2 position)
    {
        return Vector2.Distance(position, BoardRect.center) <= GatherRadius;
    }

    public void KillFriendly(AIAgent friendly)
    {
        HasFriendlyDied = true;
        Despawn(friendly);
        FriendlyDied?.Invoke();
        NotifyIfAllFriendliesGone();
    }

    public void ArriveFriendly(AIAgent friendly)
    {
        FriendliesReachedTarget++;
        Despawn(friendly);
        FriendlyWorthCollected?.Invoke(friendly.Rarity != null ? friendly.Rarity.worth : DefaultFriendlyWorth);
        FriendlyReachedTarget?.Invoke(FriendliesReachedTarget);

        if (!HasFriendlyDied && FriendliesReachedTarget == StartingFriendlyCount)
        {
            AllFriendliesReachedTarget?.Invoke();
        }

        NotifyIfAllFriendliesGone();
    }

    public void ApplySeenDestroyUpgrade(float upgradeValue)
    {
        seenDestroyDuration /= 1f + upgradeValue;
    }

    public void KillEnemy(AIAgent enemy)
    {
        Despawn(enemy);
        EnemyDestroyed?.Invoke();
    }

    private void NotifyIfAllFriendliesGone()
    {
        if (friendlies.Count == 0)
        {
            AllFriendliesGone?.Invoke();
        }
    }

    private void Despawn(AIAgent agent)
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

    private void Update()
    {
        if (!isRoundActive)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < enemySpawnInterval)
        {
            return;
        }

        spawnTimer = 0f;

        if (enemies.Count < maxEnemies && TryGetEnemySpawnPosition(out Vector2 position))
        {
            AIAgent enemy = Instantiate(agentPrefab, position, Quaternion.identity, transform);
            enemy.Initialize(AIRole.Enemy, enemyAnimationStates[Random.Range(0, enemyAnimationStates.Count)]);
            enemies.Add(enemy);
        }
    }

    private void DestroyAll(List<AIAgent> agents)
    {
        foreach (AIAgent agent in agents)
        {
            agent.gameObject.SetActive(false); // Destroy waits for end of frame, and this agent must not act until then
            Destroy(agent.gameObject);
        }

        agents.Clear();
    }

    private FriendlyRarity GetRandomRarity()
    {
        return friendlyRarities.Count > 0 ? friendlyRarities[Random.Range(0, friendlyRarities.Count)] : null;
    }

    private Vector2 GetRandomBoardPosition()
    {
        Rect area = GetCameraRect();
        return new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
    }

    private Vector2 GetFriendlySpawnPosition()
    {
        Vector2 position = Vector2.zero;

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            position = GetRandomBoardPosition();

            if (Vector2.Distance(position, TargetPosition) >= minSpawnDistanceFromTarget)
            {
                break;
            }
        }

        return position;
    }

    // Fails if no spot outside the seen zone turned up. Friendly distance is only a preference, so the farthest spot is used when none are clear.
    private bool TryGetEnemySpawnPosition(out Vector2 position)
    {
        position = Vector2.zero;
        bool found = false;
        float bestDistance = -1f;

        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            Vector2 candidate = GetRandomBoardPosition();

            if (IsInSeenZone(candidate) || Vector2.Distance(candidate, TargetPosition) < minSpawnDistanceFromTarget)
            {
                continue;
            }

            float distance = GetDistanceToNearestFriendly(candidate);

            if (distance > bestDistance)
            {
                position = candidate;
                bestDistance = distance;
                found = true;
            }

            if (distance >= minEnemySpawnDistanceFromFriendly)
            {
                break;
            }
        }

        return found;
    }

    private float GetDistanceToNearestFriendly(Vector2 position)
    {
        AIAgent nearest = FindNearestFriendly(position, float.PositiveInfinity);
        return nearest != null ? Vector2.Distance(position, nearest.transform.position) : float.PositiveInfinity;
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
            Gizmos.color = Color.cyan; // The board: spawn area and enemy wander bounds
            Gizmos.DrawWireCube(area.center, area.size);
            Gizmos.color = Color.blue; // Unseen friendlies gather and wander in here
            Gizmos.DrawWireSphere(area.center, GatherRadius);
        }

        if (target == null)
        {
            return;
        }

        Gizmos.color = Color.green; // Seen friendlies despawn inside this radius
        Gizmos.DrawWireSphere(target.position, arrivalRadius);
        Gizmos.color = Color.yellow; // Agents don't spawn inside this radius
        Gizmos.DrawWireSphere(target.position, minSpawnDistanceFromTarget);
    }
}
