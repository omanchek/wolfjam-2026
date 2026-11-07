using UnityEngine;

public enum AIRole
{
    Enemy,
    Friendly
}

public enum AIState
{
    Wandering,
    Advancing,
    Chasing,
    Attacking,
    Frozen,
    Gathering
}

public class AIAgent : MonoBehaviour
{
    [SerializeField] private AIRole role;
    [SerializeField] private bool isSeen;
    [SerializeField] private AIState state;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AIAgent prey;
    private Vector2 wanderDirection;
    private float wanderTimer;
    private float seenTimer;
    private float attackTimer;
    private float migrationTimer;

    public AIRole Role => role;
    public AIState State => state;
    public FriendlyRarity Rarity { get; private set; }

    public bool IsSeen => isSeen;

    public void Initialize(AIRole newRole, string animationState, FriendlyRarity rarity = null)
    {
        role = newRole;
        Rarity = rarity;
        animator.Play(animationState, 0, Random.value); // A random start keeps the flock from flapping in sync
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.enabled = false; // Stays hidden until its first seen check
    }

    private void Update()
    {
        isSeen = AIGameManager.Instance.IsInSeenZone(transform.position);
        spriteRenderer.enabled = isSeen;
        migrationTimer = isSeen ? AIGameManager.Instance.MigrationDuration : migrationTimer - Time.deltaTime;
        UpdatePrey();
        state = GetNextState();

        if (state != AIState.Frozen)
        {
            seenTimer = 0f; // The seen time must be continuous
        }

        if (state != AIState.Attacking)
        {
            attackTimer = 0f;
        }

        switch (state)
        {
            case AIState.Wandering:
                UpdateWandering();
                break;
            case AIState.Advancing:
                UpdateAdvancing();
                break;
            case AIState.Chasing:
                UpdateChasing();
                break;
            case AIState.Attacking:
                UpdateAttacking();
                break;
            case AIState.Frozen:
                UpdateFrozen();
                break;
            case AIState.Gathering:
                UpdateGathering();
                break;
        }
    }

    private void UpdatePrey()
    {
        if (role != AIRole.Enemy || isSeen)
        {
            prey = null;
            return;
        }

        AIGameManager manager = AIGameManager.Instance;
        AIAgent nearest = manager.FindNearestFriendly(transform.position, manager.DetectionRadius);

        if (nearest != prey)
        {
            attackTimer = 0f; // Switching victims restarts the countdown
        }

        prey = nearest;
    }

    private AIState GetNextState()
    {
        if (role == AIRole.Friendly)
        {
            if (isSeen || migrationTimer > 0f)
            {
                return AIState.Advancing;
            }

            return AIGameManager.Instance.IsInGatherZone(transform.position) ? AIState.Wandering : AIState.Gathering;
        }

        if (isSeen)
        {
            return AIState.Frozen;
        }

        if (prey == null)
        {
            return AIState.Wandering;
        }

        float distance = Vector2.Distance(transform.position, prey.transform.position);
        return distance <= AIGameManager.Instance.AttackRange ? AIState.Attacking : AIState.Chasing;
    }

    private void UpdateWandering()
    {
        AIGameManager manager = AIGameManager.Instance;
        Vector2 position = transform.position;
        Vector2 nextPosition = position + wanderDirection * (manager.WanderSpeed * Time.deltaTime);
        bool staysInBounds = role == AIRole.Friendly ? manager.IsInGatherZone(nextPosition) : manager.BoardRect.Contains(nextPosition);
        wanderTimer -= Time.deltaTime;

        if (!staysInBounds)
        {
            Vector2 toCenter = manager.BoardRect.center - position;
            wanderDirection = Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)) * toCenter.normalized; // Turn back inside the bounds
            wanderTimer = manager.WanderTurnInterval;
        }
        else if (wanderTimer <= 0f)
        {
            wanderDirection = Random.insideUnitCircle.normalized;
            wanderTimer = manager.WanderTurnInterval;
        }

        Move(wanderDirection, manager.WanderSpeed);
    }

    private void UpdateAdvancing()
    {
        AIGameManager manager = AIGameManager.Instance;
        Move(manager.TargetPosition - (Vector2)transform.position, manager.FriendlySpeed);

        if (Vector2.Distance(transform.position, manager.TargetPosition) <= manager.ArrivalRadius)
        {
            manager.ArriveFriendly(this);
        }
    }

    private void UpdateGathering()
    {
        AIGameManager manager = AIGameManager.Instance;
        Move(manager.TargetPosition - (Vector2)transform.position, manager.WanderSpeed);
    }

    private void UpdateChasing()
    {
        Move(prey.transform.position - transform.position, AIGameManager.Instance.EnemySpeed);
    }

    private void UpdateAttacking()
    {
        attackTimer += Time.deltaTime;
        Vector2 toPrey = prey.transform.position - transform.position;

        if (toPrey.magnitude > AIGameManager.Instance.AttackRange * 0.5f)
        {
            Move(toPrey, AIGameManager.Instance.EnemySpeed); // Keep up so walking prey can't reset the timer
        }

        if (attackTimer >= AIGameManager.Instance.AttackDuration)
        {
            AIGameManager.Instance.KillFriendly(prey);
            prey = null;
        }
    }

    private void UpdateFrozen()
    {
        seenTimer += Time.deltaTime;

        if (seenTimer >= AIGameManager.Instance.SeenDestroyDuration)
        {
            AIGameManager.Instance.KillEnemy(this);
        }
    }

    private void Move(Vector2 direction, float speed)
    {
        if (direction == Vector2.zero)
        {
            return;
        }

        transform.up = direction;
        transform.position += (Vector3)(direction.normalized * (speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        AIGameManager manager = AIGameManager.Instance;

        if (manager == null)
        {
            return;
        }

        Gizmos.color = GetStateColor();

        if (prey != null)
        {
            Gizmos.DrawLine(transform.position, prey.transform.position);
        }
        else if (state == AIState.Advancing)
        {
            Gizmos.DrawLine(transform.position, manager.TargetPosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        AIGameManager manager = AIGameManager.Instance;

        if (manager == null || role != AIRole.Enemy)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, manager.DetectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, manager.AttackRange);
    }

    private Color GetStateColor()
    {
        switch (state)
        {
            case AIState.Advancing:
                return Color.green;
            case AIState.Chasing:
                return Color.magenta;
            case AIState.Attacking:
                return Color.red;
            default:
                return Color.gray;
        }
    }
}
