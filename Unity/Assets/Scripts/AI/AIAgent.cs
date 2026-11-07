using UnityEngine;

public enum AIRole
{
    Enemy,
    Friendly
}

public enum AIState
{
    Idle,
    Advancing,
    Retreating
}

public class AIAgent : MonoBehaviour
{
    [SerializeField] private AIRole role;
    [SerializeField] private bool isSeen;
    [SerializeField] private AIState state;

    private Renderer agentRenderer;

    public AIRole Role => role;
    public AIState State => state;

    public bool IsSeen
    {
        get => isSeen;
        set => isSeen = value;
    }

    public void Initialize(AIRole newRole, bool seen)
    {
        role = newRole;
        isSeen = seen;
    }

    private void Awake()
    {
        agentRenderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        state = GetNextState();

        switch (state)
        {
            case AIState.Advancing:
                UpdateAdvancing();
                break;
            case AIState.Retreating:
                UpdateRetreating();
                break;
        }
    }

    private AIState GetNextState()
    {
        if (!isSeen)
        {
            return AIState.Idle;
        }

        return role == AIRole.Friendly ? AIState.Advancing : AIState.Retreating;
    }

    private void UpdateAdvancing()
    {
        AIGameManager manager = AIGameManager.Instance;
        Vector2 toTarget = manager.TargetPosition - (Vector2)transform.position;
        Move(toTarget, toTarget, manager.FriendlySpeed);

        if (Vector2.Distance(transform.position, manager.TargetPosition) <= manager.ArrivalRadius)
        {
            manager.Despawn(this);
        }
    }

    private void UpdateRetreating()
    {
        AIGameManager manager = AIGameManager.Instance;
        Vector2 toTarget = manager.TargetPosition - (Vector2)transform.position;
        Move(toTarget, -toTarget, manager.EnemySpeed); // Backs away while still facing the target

        if (manager.IsCompletelyOffscreen(agentRenderer.bounds))
        {
            manager.Despawn(this);
        }
    }

    private void Move(Vector2 facing, Vector2 direction, float speed)
    {
        transform.up = facing;
        transform.position += (Vector3)(direction.normalized * (speed * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        if (AIGameManager.Instance == null)
        {
            return;
        }

        Gizmos.color = GetStateColor();
        Gizmos.DrawLine(transform.position, AIGameManager.Instance.TargetPosition);
    }

    private Color GetStateColor()
    {
        switch (state)
        {
            case AIState.Advancing:
                return Color.green;
            case AIState.Retreating:
                return Color.red;
            default:
                return Color.gray;
        }
    }
}
