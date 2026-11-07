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

    [SerializeField, Min(0)] private int friendliesPerRound = 8;
    [SerializeField] private List<float> cameraSizes = new List<float> { 5f, 7f, 10f }; // One per round, the last repeats
    [SerializeField, Min(0f)] private float cameraTransitionDuration = 2f;

    private GlobalData globalData;
    private Tween cameraTween;

    public int RoundNumber { get; private set; }
    public GamePhase Phase { get; private set; }

    public int FriendliesPerRound
    {
        get => friendliesPerRound;
        set => friendliesPerRound = Mathf.Max(0, value);
    }

    public event Action GameOver;
    public event Action UpgradeChoiceRequested; // The upgrade UI should open on this

    public void StartNewRound()
    {
        cameraTween?.Kill();
        RoundNumber++;

        if (cameraSizes.Count > 0)
        {
            Camera.main.orthographicSize = GetCameraSizeForRound(RoundNumber);
        }

        Phase = GamePhase.Playing;
        AIGameManager.Instance.StartRound(friendliesPerRound);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AIGameManager.Instance.FriendlyDied += HandleFriendlyDied;
        AIGameManager.Instance.AllFriendliesReachedTarget += HandleAllFriendliesReachedTarget;
        globalData = GlobalData.GetInstance();
        globalData.onUpgradeChoiceFinished += HandleUpgradeChoiceFinished;
        StartNewRound();
    }

    private void OnDestroy()
    {
        cameraTween?.Kill();

        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.FriendlyDied -= HandleFriendlyDied;
            AIGameManager.Instance.AllFriendliesReachedTarget -= HandleAllFriendliesReachedTarget;
        }

        if (globalData != null)
        {
            globalData.onUpgradeChoiceFinished -= HandleUpgradeChoiceFinished;
        }
    }

    private void HandleFriendlyDied()
    {
        if (Phase == GamePhase.GameOver)
        {
            return;
        }

        cameraTween?.Kill();
        Phase = GamePhase.GameOver;
        AIGameManager.Instance.StopRound();
        GameOver?.Invoke();
    }

    private void HandleAllFriendliesReachedTarget()
    {
        AIGameManager.Instance.StopRound();
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
