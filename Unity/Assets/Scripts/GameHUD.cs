using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    private static readonly UpgradeSet.UpgradeTypes[] OfferedUpgradeTypes =
    {
        UpgradeSet.UpgradeTypes.UP_Speed,
        UpgradeSet.UpgradeTypes.UP_Intensity,
        UpgradeSet.UpgradeTypes.UP_Area,
    };
    private static readonly float[] UpgradeValues = { 0.5f, 0.75f, 1.0f };

    //components
    [Header("Light")]
    [SerializeField] TextMeshProUGUI lightScoreText;
    [SerializeField] LightManager lightManager;

    [Header("Pause")]
    [SerializeField] GameObject pausePanel;

    [Header("Quota")]
    [SerializeField] TextMeshProUGUI quotaText;

    [Header("Upgrades")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject upgradeButton1;
    [SerializeField] private GameObject upgradeButton2;
    [SerializeField] Dictionary<UpgradeSet.UpgradeTypes, string> upgradeTypeToDisplayName;
    [SerializeField] UpgradeSet upgradeSet;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Tutorial")]
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] TextMeshProUGUI tutorialTitle;
    [SerializeField] TextMeshProUGUI tutorialTip;
    [SerializeField] TutorialTrigger[] tutorialSteps;
    private int tutorialStep = -1;

    //members
    GlobalData globalData;
    float lightEffectTimer = 0.0f;
    UpgradeSet.UpgradeContainer upgrade1;
    UpgradeSet.UpgradeContainer upgrade2;

    UpgradeSet.UpgradeContainer speedUpgrade1;
    UpgradeSet.UpgradeContainer speedUpgrade2;

    UpgradeSet.UpgradeContainer intensityUpgrade1;
    UpgradeSet.UpgradeContainer intensityUpgrade2;
    UpgradeSet.UpgradeContainer intensityUpgrade3;

    UpgradeSet.UpgradeContainer areaUpgrade1;
    UpgradeSet.UpgradeContainer areaUpgrade2;
    UpgradeSet.UpgradeContainer areaUpgrade3;

    private void Awake()
    {
        globalData = GlobalData.GetInstance();
        globalData.onLightScoreUpdated += OnLightScoreUpdated;
        globalData.onTutorialTextUpdated += OnTutorialTextUpdated;

        upgradePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        AddUpgradeOptions();
    }

    private void Start()
    {
        GameManager.Instance.UpgradeChoiceRequested += GenerateUpgrades;
        GameManager.Instance.RoundStarted += UpdateQuotaText;
        GameManager.Instance.GameOver += ShowGameOver;
        GlobalData.GetInstance().GetSoundManager().PlaySoundByTypeAndID(SoundManager.SoundTypes.MUSIC, "level-theme");
    }

    private void OnDestroy()
    {
        // GetInstance would create a new object here if GlobalData was already destroyed on quit
        if (globalData != null)
        {
            globalData.onLightScoreUpdated -= OnLightScoreUpdated;
            globalData.onTutorialTextUpdated -= OnTutorialTextUpdated;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpgradeChoiceRequested -= GenerateUpgrades;
            GameManager.Instance.RoundStarted -= UpdateQuotaText;
            GameManager.Instance.GameOver -= ShowGameOver;
        }
    }

    public void StepTutorial()
    {
        tutorialStep++;
        if (tutorialStep >= tutorialSteps.Length) tutorialPanel.SetActive(false);
        else
        {
            tutorialTitle.text = tutorialSteps[tutorialStep].GetTutorialTitle();
            tutorialTip.text = tutorialSteps[tutorialStep].GetTutorialText();
        }            
    }

    private void AddUpgradeOptions()
    {
        speedUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Speed, 0.005f);
        speedUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Speed, 0.01f);

        intensityUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 0.1f);
        intensityUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 0.2f);
        intensityUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 0.3f);

        areaUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 0.50f);
        areaUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 0.75f);
        areaUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 1.0f);

        upgradeSet.AddUpgradeOption(speedUpgrade1);
        upgradeSet.AddUpgradeOption(speedUpgrade2);

        upgradeSet.AddUpgradeOption(intensityUpgrade1);
        upgradeSet.AddUpgradeOption(intensityUpgrade2);
        upgradeSet.AddUpgradeOption(intensityUpgrade3);

        upgradeSet.AddUpgradeOption(areaUpgrade1);
        upgradeSet.AddUpgradeOption(areaUpgrade2);
        upgradeSet.AddUpgradeOption(areaUpgrade3);
    }

    private void CloseUpgradesMenu()
    {
        upgradePanel.SetActive(false);
        GlobalData.GetInstance().FinishUpgradeChoice();
    }

    private void GenerateUpgrades()
    {
        // Picks two different types, skipping UP_None at index 0
        int typeCount = upgradeTypeToDisplayName.Count;
        int opt1 = Random.Range(1, typeCount);
        int opt2 = Random.Range(1, typeCount - 1);

        //ensure no overlap, with every pairing equally likely
        if (opt2 >= opt1)
        {
            opt2++;
        }

        //init buttons
        upgradeButton1.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt1];
        upgradeButton2.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt2];

        upgrade1 = upgradeSet.GetUpgradeSpecific((UpgradeSet.UpgradeTypes)opt1);        
        upgrade2 = upgradeSet.GetUpgradeSpecific((UpgradeSet.UpgradeTypes)opt2);

        //activate upgrade panel
        upgradePanel.SetActive(true);
    }

    private void OnLightScoreUpdated(int score, bool isIncrease)
    {
        lightScoreText.text = score.ToString();
        UpdateQuotaText();
    }

    private void UpdateQuotaText()
    {
        GameManager manager = GameManager.Instance;
        quotaText.text = $"Round {manager.RoundNumber}/{manager.TotalRounds}\nQuota {manager.QuotaProgress}/{manager.CurrentQuota}";
    }

    private void ShowGameOver()
    {
        GameManager manager = GameManager.Instance;
        gameOverText.text = manager.ClearedAllRounds
            ? "All rounds cleared!"
            : $"Quota missed: {manager.QuotaProgress}/{manager.CurrentQuota}";
        gameOverPanel.SetActive(true);
    }

    public void OnPlayAgain()
    {
        SceneSwitcher.SwitchToScene_Static(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenu()
    {
        SceneSwitcher.SwitchToScene_Static(mainMenuSceneName);
    }

    public void OnSelectUpgrade(int index)
    {
        UpgradeSet.UpgradeContainer chosen = index == 0 ? upgrade1 : upgrade2;
        upgradeSet.AddUpgrade(chosen);
        ApplyUpgrade(chosen);
        CloseUpgradesMenu();
    }

    private void ApplyUpgrade(UpgradeSet.UpgradeContainer upgrade)
    {
        switch (upgrade.mType)
        {
            case UpgradeSet.UpgradeTypes.UP_Intensity:
                AIGameManager.Instance.ApplySeenDestroyUpgrade(upgrade.mValue);
                break;
            case UpgradeSet.UpgradeTypes.UP_Area:
                lightManager.IncreaseRadiusF(upgrade.mValue);
                GameManager.Instance.ApplyAreaUpgrade(upgrade.mValue);
                break;
            case UpgradeSet.UpgradeTypes.UP_Speed:
                lightManager.ApplySpeedUpgrade(upgrade.mValue);
                break;
        }
    }

    private void OnTutorialTextUpdated(TutorialTrigger trigger)
    {
        Debug.LogFormat("called {0}", GlobalData.GetInstance().GetTutorialStep());
        if (tutorialPanel != null)
        {
            tutorialTip.text = trigger.GetTutorialText();
            tutorialTitle.text = trigger.GetTutorialTitle();
        }
    }

    public void ToggleGamePaused(bool isPaused)
    {
        if (isPaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }
}
