using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum GamePhase
{
    Playing,
    CameraTransition,
    WaitingForUpgrade,
    GameOver
}

[DefaultExecutionOrder(100)] // The first round starts after other scripts have subscribed in their Start
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Min(1)] private int totalRounds = 5;
    [SerializeField, Min(0)] private int friendliesPerRound = 8;
    [SerializeField] private List<float> cameraSizes = new List<float> { 5f, 7f, 10f }; // One per round, the last repeats
    [SerializeField, Min(0f)] private float cameraTransitionDuration = 2f;
    [SerializeField] private List<int> roundQuotas = new List<int> { 4, 5, 6 }; // One per round, the last repeats

    private GlobalData globalData;
    private Tween cameraTween;

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
        cameraTween?.Kill();
        RoundNumber++;

        if (cameraSizes.Count > 0)
        {
            Camera.main.orthographicSize = GetCameraSizeForRound(RoundNumber);
        }

        Phase = GamePhase.Playing;
        CurrentQuota = roundQuotas.Count > 0 ? roundQuotas[Mathf.Min(RoundNumber - 1, roundQuotas.Count - 1)] : 0;
        QuotaProgress = 0;
        RoundStarted?.Invoke();
        AIGameManager.Instance.StartRound(friendliesPerRound);
    }

    private void Awake()
    {
        Instance = this;
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

        Phase = GamePhase.CameraTransition;
        float size = cameraSizes.Count > 0 ? GetCameraSizeForRound(RoundNumber + 1) : Camera.main.orthographicSize;
        cameraTween = Camera.main.DOOrthoSize(size, cameraTransitionDuration).OnComplete(RequestUpgradeChoice);
    }

    private void RequestUpgradeChoice()
    {
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

    private float GetCameraSizeForRound(int round)
    {
        return cameraSizes[Mathf.Min(round - 1, cameraSizes.Count - 1)];
    }
}
