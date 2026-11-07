using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using NUnit.Framework.Internal;

public class LightManager : MonoBehaviour
{
    [SerializeField] private Transform lightArea;
    Vector3 mousePos;
    private List<GameObject> listTest = new List<GameObject>();


    private void Update()
    {
        listTest.Clear();
        AddObj();
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
        transform.position = worldPos;
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
}
