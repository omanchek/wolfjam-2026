using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField, Min(0f)] private float arrivalRadius = 0.75f;

    [Header("Spawning")]
    [SerializeField, Min(0.01f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(0)] private int maxAgents = 20;
    [SerializeField, Range(0f, 1f)] private float friendlyChance = 0.5f;
    [SerializeField, Min(0f)] private float minSpawnDistanceFromTarget = 2f;
    [SerializeField] private bool spawnSeen = true; // Nothing sets visibility yet, so agents default to seen

    [Header("Appearance")]
    [SerializeField] private Color friendlyColor = Color.green;
    [SerializeField] private Color enemyColor = Color.red;

    private readonly List<AIAgent> agents = new List<AIAgent>();
    private Camera mainCamera;
    private float spawnTimer;

    public float FriendlySpeed => friendlySpeed;
    public float EnemySpeed => enemySpeed;
    public float ArrivalRadius => arrivalRadius;
    public Vector2 TargetPosition => target.position;

    public void Despawn(AIAgent agent)
    {
        agents.Remove(agent);
        Destroy(agent.gameObject);
    }

    public bool IsCompletelyOffscreen(Bounds bounds)
    {
        Rect boundsRect = Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
        return !GetCameraRect().Overlaps(boundsRect);
    }

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer = 0f;

        if (agents.Count < maxAgents)
        {
            SpawnAgent();
        }
    }

    private void SpawnAgent()
    {
        AIRole role = Random.value < friendlyChance ? AIRole.Friendly : AIRole.Enemy;
        AIAgent agent = Instantiate(agentPrefab, GetSpawnPosition(), Quaternion.identity, transform);
        agent.Initialize(role, spawnSeen);
        agent.GetComponentInChildren<SpriteRenderer>().color = role == AIRole.Friendly ? friendlyColor : enemyColor;
        agents.Add(agent);
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
        if (Camera.main != null)
        {
            Rect area = GetCameraRect();
            Gizmos.color = Color.cyan; // Spawn area, and the bounds enemies despawn past
            Gizmos.DrawWireCube(area.center, area.size);
        }

        if (target == null)
        {
            return;
        }

        Gizmos.color = friendlyColor; // Friendlies despawn inside this radius
        Gizmos.DrawWireSphere(target.position, arrivalRadius);
        Gizmos.color = Color.yellow; // Agents don't spawn inside this radius
        Gizmos.DrawWireSphere(target.position, minSpawnDistanceFromTarget);
    }
}
