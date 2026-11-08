using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// Pulses the lighthouse when a friendly arrives and adds a moth to an evenly spaced oval orbit around its light
public class LighthouseOrbit : MonoBehaviour
{
    [SerializeField] private GameObject mothVisual; // The agent prefab's sprite child, copied for each orbiting moth
    [SerializeField] private string mothAnimationState = "MothFlying";
    [SerializeField] private Color mothColor = Color.yellow;
    [SerializeField, Min(0f)] private float mothScale = 0.5f;
    [SerializeField, Range(0f, 1f)] private float backScale = 0.8f; // Moths shrink to this fraction at the far side
    [SerializeField] private int frontSortingOrder = 11; // Both orders must stay above the darkness overlay
    [SerializeField] private int backSortingOrder = 9;
    [SerializeField] private Vector2 orbitCenterOffset = new Vector2(0f, 2f); // From the lighthouse's pivot to its light
    [SerializeField] private Vector2 orbitRadii = new Vector2(40f, 14f); // A flat oval reads as a circle seen at an angle
    [SerializeField] private float orbitDegreesPerSecond = 45f;
    [SerializeField, Min(0f)] private float effectDuration = 0.4f;
    [SerializeField, Min(0f)] private float pulseStrength = 0.15f; // Fraction of the lighthouse's size

    private readonly List<Transform> moths = new List<Transform>();
    private readonly List<SpriteRenderer> mothRenderers = new List<SpriteRenderer>();
    private float orbitAngle;

    private void OnEnable()
    {
        AIGameManager.Instance.FriendlyReachedTarget += HandleFriendlyReachedTarget;
        AIGameManager.Instance.FriendliesSpawned += HandleFriendliesSpawned;
    }

    private void OnDisable()
    {
        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.FriendlyReachedTarget -= HandleFriendlyReachedTarget;
            AIGameManager.Instance.FriendliesSpawned -= HandleFriendliesSpawned;
        }
    }

    private void OnDestroy()
    {
        transform.DOKill(true);
    }

    private void Update()
    {
        orbitAngle += orbitDegreesPerSecond * Time.deltaTime;

        for (int i = 0; i < moths.Count; i++)
        {
            float angle = (orbitAngle + 360f * i / moths.Count) * Mathf.Deg2Rad;
            float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
            moths[i].position = (Vector2)transform.position + orbitCenterOffset + new Vector2(cos * orbitRadii.x, sin * orbitRadii.y);
            moths[i].up = new Vector2(-sin * orbitRadii.x, cos * orbitRadii.y) * Mathf.Sign(orbitDegreesPerSecond); // Faces the way it is flying

            // The lower half of the oval is nearer the viewer, so it draws over the lighthouse
            mothRenderers[i].sortingOrder = sin < 0f ? frontSortingOrder : backSortingOrder;

            if (!DOTween.IsTweening(moths[i]))
            {
                moths[i].localScale = Vector3.one * (mothScale * Mathf.Lerp(1f, backScale, (sin + 1f) * 0.5f));
            }
        }
    }

    private void HandleFriendlyReachedTarget(int totalReached)
    {
        transform.DOComplete();
        transform.DOPunchScale(transform.localScale * pulseStrength, effectDuration, 1, 0f);
        AddMoth();
    }

    private void HandleFriendliesSpawned(int startingCount)
    {
        foreach (Transform moth in moths)
        {
            moth.DOKill();
            Destroy(moth.gameObject);
        }

        moths.Clear();
        mothRenderers.Clear();
    }

    private void AddMoth()
    {
        Transform moth = new GameObject("OrbitMoth").transform;
        moth.SetParent(transform.parent, false); // A sibling, so the lighthouse's pulse and scale don't stretch it
        GameObject visual = Instantiate(mothVisual, moth, false);

        SpriteRenderer spriteRenderer = visual.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
        spriteRenderer.color = mothColor;
        visual.GetComponent<Animator>().Play(mothAnimationState, 0, Random.value);

        moths.Add(moth);
        mothRenderers.Add(spriteRenderer);
        moth.localScale = Vector3.zero;
        moth.DOScale(mothScale, effectDuration).SetEase(Ease.OutBack); // Pops in
    }
}
