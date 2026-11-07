using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using NUnit.Framework.Internal;
using System.ComponentModel;

public class LightManager : MonoBehaviour
{
    Vector3 mousePos;
    private List<GameObject> listTest = new List<GameObject>();
    [SerializeField] AIGameManager aiGameManager;
    PlayerActions playerActions;

    [Header("Stats")]
    [SerializeField] Color startColor = Color.white;
    [SerializeField] Color endColor = Color.red;
    private float intensityUpgradeVal = 0;
    private float maxIntensity = 3.0f;
    private float movementSpeed = 0.005f;
    public float GetIntensityUpgradeVal => intensityUpgradeVal;

    private void Awake()
    {
        playerActions = new PlayerActions();

        playerActions.MouseActions.IncreaseRadius.performed += IncreaseRadius;
        playerActions.MouseActions.DecreaseRadius.performed += DecreaseRadius;
    }

    private void Update()
    {
        listTest.Clear();
        AddObj();
        UpdateLightState();
    }

    private void FixedUpdate()
    {
        UpdateLightPos();
    }

    private void AddObj()
    {
        GameObject[] objects = FindObjectsByType<GameObject>();

        foreach (var obj in objects)
        {
            if (obj.tag == "AI")
            {
                listTest.Add(obj);
            }
        }
    }

    private void UpdateLightPos()
    {
        mousePos = Pointer.current.position.ReadValue();
        mousePos.z = 10; // <-- NOTE: THIS VALUE NEEDS TO BE SET BASED ON HOW ZOOMED OUT THE CAMERA IS
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = Vector2.Lerp(transform.position, worldPos, movementSpeed);
    }

    private void UpdateLightState()
    {
        // Updates the zone that the enemy can see
        aiGameManager.SetSeenZone(this.gameObject.transform.position, this.gameObject.GetComponent<CircleCollider2D>().radius * this.transform.localScale.x);
    }

    public void IncreaseRadius(InputAction.CallbackContext context)
    {
        intensityUpgradeVal += 0.3f;    }

    public void IncreaseRadiusV3(Vector3 increaseAmount)
    {
        // Increase by a vector
        this.gameObject.transform.localScale += increaseAmount;
    }

    public void IncreaseRadiusF(float increaseAmount)
    {
        // Increase components (x && y) by a desired amount
        Vector3 localScale = this.gameObject.transform.localScale;
        localScale.x += increaseAmount;
        localScale.y += increaseAmount;
        this.gameObject.transform.localScale = localScale;
    }

    private void DecreaseRadius(InputAction.CallbackContext context)
    {
        // Decreases the size of the light obj
        this.gameObject.transform.localScale -= new Vector3(0.5f, 0.5f, 0.0f);
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
}
