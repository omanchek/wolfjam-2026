using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using NUnit.Framework.Internal;
using System.ComponentModel;

public class LightManager : MonoBehaviour
{
    [SerializeField] AIGameManager aiGameManager;
    [SerializeField, Min(0f)] float moveSpeed = 15f;
    [SerializeField, Min(0f)] float maxRangeFromTarget = 8f;

    PlayerActions playerActions;
    CircleCollider2D lightCollider;

    [Header("Stats")]
    [SerializeField] Color startColor = Color.white;
    [SerializeField] Color endColor = Color.red;
    private float intensityUpgradeVal = 0;
    private float maxIntensity = 3.0f;
    private float movementSpeed = 0.005f;
    public float GetIntensityUpgradeVal => intensityUpgradeVal;

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
        transform.localScale += new Vector3(0.5f, 0.5f, 0.0f);
	}

    public void IncreaseRadiusF(float increaseAmount)
    {
        // Increase components (x && y) by a desired amount
        Vector3 localScale = transform.localScale;
        localScale.x += increaseAmount;
        localScale.y += increaseAmount;
        transform.localScale = localScale;
    }

    private void DecreaseRadius(InputAction.CallbackContext context)
    {
        // Decreases the size of the light obj
        transform.localScale -= new Vector3(0.5f, 0.5f, 0.0f);
	}

    public void IncreaseIntensity(float increaseAmount)
    {
        intensityUpgradeVal += increaseAmount;
        //GetComponentInChildren<SpriteRenderer>().color = Color.Lerp(startColor, endColor, (intensityUpgradeVal / maxIntensity));
    }

    public void IncreaseMovementSpeed(float increaseAmount)
    {
        movementSpeed += increaseAmount;
    }

    // DOES NOT WORK, NOT SURE WHY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision Entered");
        if (collision.gameObject.tag == "AI")
        {
            Debug.Log(collision.gameObject.name);
        }
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
