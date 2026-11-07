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
        UpgradeSet.UpgradeTypes.UP_Range
    };
    private static readonly float[] UpgradeValues = { 0.5f, 0.75f, 1.0f };

    //components
    [Header("Light")]
    [SerializeField] TextMeshProUGUI lightScoreText;
    [SerializeField] Image lightCollectionEffect;
    [SerializeField] Color lightEffectColor;
    [SerializeField] float lightEffectTime = 0.25f;
    [SerializeField] float lightEffectMaxIntensity = 0.75f;
    [SerializeField] LightManager lightManager;

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

    //members
    float lightEffectTimer = 0.0f;
    UpgradeSet.UpgradeContainer upgrade1;
    UpgradeSet.UpgradeContainer upgrade2;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
        GlobalData.GetInstance().onTutorialTextUpdated += OnTutorialTextUpdated;

        upgradePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        AddUpgradeOptions();
    }

    private void Start()
    {
        GameManager.Instance.UpgradeChoiceRequested += GenerateUpgrades;
        GameManager.Instance.RoundStarted += UpdateQuotaText;
        GameManager.Instance.GameOver += ShowGameOver;
    }

    private void OnDestroy()
    {
        GlobalData.GetInstance().onLightScoreUpdated -= OnLightScoreUpdated;
        GlobalData.GetInstance().onTutorialTextUpdated -= OnTutorialTextUpdated;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpgradeChoiceRequested -= GenerateUpgrades;
            GameManager.Instance.RoundStarted -= UpdateQuotaText;
            GameManager.Instance.GameOver -= ShowGameOver;
        }
    }

    private void Update()
    {
        UpdateLightEffect();
    }

    private void BeginLightEffect()
    {
        lightEffectTimer = lightEffectTime;
    }

    private void AddUpgradeOptions()
    {
        foreach (UpgradeSet.UpgradeTypes type in OfferedUpgradeTypes)
        {
            foreach (float value in UpgradeValues)
            {
                upgradeSet.AddUpgradeOption(new UpgradeSet.UpgradeContainer(type, value));
            }
        }
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

        if (isIncrease)
        {
            BeginLightEffect();
        }
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
            case UpgradeSet.UpgradeTypes.UP_Range:
                lightManager.ApplyRangeUpgrade(upgrade.mValue);
                break;
            case UpgradeSet.UpgradeTypes.UP_Area:
                lightManager.IncreaseRadiusF(upgrade.mValue);
                break;
            case UpgradeSet.UpgradeTypes.UP_Speed:
                lightManager.ApplySpeedUpgrade(upgrade.mValue);
                break;
        }
    }

    private void OnTutorialTextUpdated(string text)
    {
        Debug.Log(text);
    }

    private void UpdateLightEffect()
    {
        //check if running
        if (lightEffectTimer > 0.0f)
        {
            lightEffectTimer -= Time.deltaTime;
            lightCollectionEffect.color = new Color(lightEffectColor.r, lightEffectColor.g, lightEffectColor.b,
                                                    Mathf.Lerp(0.0f, lightEffectMaxIntensity, lightEffectTimer / lightEffectTime));
        }
    }
}
