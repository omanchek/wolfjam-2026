using UnityEngine;
using UnityEngine.InputSystem;

public class LightManager : MonoBehaviour
{
    [SerializeField] AIGameManager aiGameManager;
    [SerializeField, Min(0f)] float moveSpeed = 450f;
    [SerializeField, Min(0f)] float maxRangeFromTarget = 240f;
    [SerializeField, Min(0f)] float radiusStep = 30f; // How much one point of area upgrade adds to the light's scale

    PlayerActions playerActions;
    CircleCollider2D lightCollider;

    private void Awake()
    {
        lightCollider = GetComponent<CircleCollider2D>();
        playerActions = new PlayerActions();

        playerActions.MouseActions.IncreaseRadius.performed += IncreaseRadius;
        playerActions.MouseActions.DecreaseRadius.performed += DecreaseRadius;
    }

    private void Update()
    {
        UpdateLightPos();
        UpdateLightState();
    }

    private void UpdateLightPos()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
        Vector2 targetPos = aiGameManager.TargetPosition;
        Vector2 allowedPos = targetPos + Vector2.ClampMagnitude(mouseWorldPos - targetPos, maxRangeFromTarget);
        transform.position = Vector2.MoveTowards(transform.position, allowedPos, moveSpeed * Time.deltaTime);
    }

    private void UpdateLightState()
    {
        // Updates the zone that the enemy can see
        aiGameManager.SetSeenZone(transform.position, lightCollider.radius * transform.localScale.x);
    }

    public void ApplySpeedUpgrade(float upgradeValue)
    {
        moveSpeed *= 1f + upgradeValue;
    }

    public void ApplyRangeUpgrade(float upgradeValue)
    {
        maxRangeFromTarget *= 1f + upgradeValue;
    }

    private void IncreaseRadius(InputAction.CallbackContext context)
    {
        // Increases the size of the light obj
        IncreaseRadiusF(0.5f);
    }

    public void IncreaseRadiusF(float increaseAmount)
    {
        // Increase components (x && y) by a desired amount
        Vector3 localScale = transform.localScale;
        localScale.x += increaseAmount * radiusStep;
        localScale.y += increaseAmount * radiusStep;
        transform.localScale = localScale;
    }

    private void DecreaseRadius(InputAction.CallbackContext context)
    {
        // Decreases the size of the light obj
        IncreaseRadiusF(-0.5f);
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    private void OnDestroy()
    {
        playerActions.MouseActions.IncreaseRadius.performed -= IncreaseRadius;
        playerActions.MouseActions.DecreaseRadius.performed -= DecreaseRadius;
        playerActions.Dispose();
    }
}
