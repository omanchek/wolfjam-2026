using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GamePhase
{
    Playing,
    WaitingForUpgrade,
    GameOver
}

[DefaultExecutionOrder(100)] // The first round starts after other scripts have subscribed in their Start
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Min(1)] private int totalRounds = 5;
    [SerializeField, Min(0)] private int friendliesPerRound = 8;
    [SerializeField, Min(0f)] private float cameraTransitionDuration = 2f;
    [SerializeField, Min(0f)] private float cameraSizePerAreaUpgrade = 100f; // Added per point of upgrade value
    [SerializeField] private SpriteRenderer background; // The camera never shows past this
    [SerializeField] private List<int> roundQuotas = new List<int> { 4, 5, 6 }; // One per round, the last repeats

    private GlobalData globalData;
    private Tween cameraTween;
    private float startCameraSize;
    private Vector3 startCameraPosition;

    public int TotalRounds => totalRounds;
    public int RoundNumber { get; private set; }
    public GamePhase Phase { get; private set; }
    public int CurrentQuota { get; private set; }
    public int QuotaProgress { get; private set; }
    public bool ClearedAllRounds { get; private set; } // Tells a finished run apart from a missed quota at game over

    public int FriendliesPerRound
    {
        get => friendliesPerRound;
        set => friendliesPerRound = Mathf.Max(0, value);
    }

    public event Action GameOver;
    public event Action UpgradeChoiceRequested; // The upgrade UI should open on this
    public event Action RoundStarted;

    public void StartNewRound()
    {
        RoundNumber++;
        Phase = GamePhase.Playing;
        CurrentQuota = roundQuotas.Count > 0 ? roundQuotas[Mathf.Min(RoundNumber - 1, roundQuotas.Count - 1)] : 0;
        QuotaProgress = 0;
        RoundStarted?.Invoke();
        AIGameManager.Instance.StartRound(friendliesPerRound);
    }

    public void ApplyAreaUpgrade(float upgradeValue)
    {
        IncreaseCameraSize(upgradeValue * cameraSizePerAreaUpgrade);
    }

    // Finishes any zoom still running so repeat calls add up
    public void IncreaseCameraSize(float amount)
    {
        cameraTween?.Complete();
        Camera camera = Camera.main;
        float newSize = camera.orthographicSize + amount;
        Vector3 newPosition = camera.transform.position;

        if (background != null)
        {
            // The camera drifts to the background's center, so the view fills the image exactly at max size
            Bounds bounds = background.bounds;
            float maxSize = Mathf.Min(bounds.extents.y, bounds.extents.x / camera.aspect);
            newSize = Mathf.Min(newSize, maxSize);
            Vector3 maxPosition = new Vector3(bounds.center.x, bounds.center.y, startCameraPosition.z);
            newPosition = Vector3.Lerp(startCameraPosition, maxPosition, Mathf.InverseLerp(startCameraSize, maxSize, newSize));
        }

        cameraTween = DOTween.Sequence()
            .Join(camera.DOOrthoSize(newSize, cameraTransitionDuration))
            .Join(camera.transform.DOMove(newPosition, cameraTransitionDuration));
    }

    private void Awake()
    {
        Instance = this;
        startCameraSize = Camera.main.orthographicSize;
        startCameraPosition = Camera.main.transform.position;
    }

    private void Start()
    {
        AIGameManager.Instance.FriendlyWorthCollected += HandleFriendlyWorthCollected;
        AIGameManager.Instance.AllFriendliesGone += HandleAllFriendliesGone;
        globalData = GlobalData.GetInstance();
        globalData.onUpgradeChoiceFinished += HandleUpgradeChoiceFinished;
        globalData.SetLightScore(0); // GlobalData outlives the scene, so a new run must clear the last score
        StartNewRound();
    }

    private void OnDestroy()
    {
        cameraTween?.Kill();

        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.FriendlyWorthCollected -= HandleFriendlyWorthCollected;
            AIGameManager.Instance.AllFriendliesGone -= HandleAllFriendliesGone;
        }

        if (globalData != null)
        {
            globalData.onUpgradeChoiceFinished -= HandleUpgradeChoiceFinished;
        }
    }

    private void HandleFriendlyWorthCollected(int worth)
    {
        QuotaProgress += worth;
        globalData.IncrementLightScore(worth);
    }

    private void HandleAllFriendliesGone()
    {
        AIGameManager.Instance.StopRound();

        bool quotaMet = QuotaProgress >= CurrentQuota;
        ClearedAllRounds = quotaMet && RoundNumber >= totalRounds;

        if (!quotaMet || ClearedAllRounds)
        {
            Phase = GamePhase.GameOver;
            GameOver?.Invoke();
            return;
        }

        Phase = GamePhase.WaitingForUpgrade;
        UpgradeChoiceRequested?.Invoke();
    }

    private void HandleUpgradeChoiceFinished()
    {
        if (Phase == GamePhase.WaitingForUpgrade)
        {
            StartNewRound();
        }
    }
}
