using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    //components
    [Header("Light")]
    [SerializeField] TextMeshProUGUI lightScoreText;
    [SerializeField] Image lightCollectionEffect;
    [SerializeField] Color lightEffectColor;
    [SerializeField] float lightEffectTime = 0.25f;
    [SerializeField] float lightEffectMaxIntensity = 0.75f;
    [SerializeField] LightManager lightManager;

    [Header("Upgrades")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject upgradeButton1;
    [SerializeField] private GameObject upgradeButton2;
    [SerializeField] Dictionary<UpgradeSet.UpgradeTypes, string> upgradeTypeToDisplayName;

    [Header("Debug")]
    [SerializeField] UpgradeSet upgradeSet;

    //members
    float lightEffectTimer = 0.0f;
    UpgradeSet.UpgradeContainer upgrade1;
    UpgradeSet.UpgradeContainer upgrade2;

    UpgradeSet.UpgradeContainer speedUpgrade1;
    UpgradeSet.UpgradeContainer speedUpgrade2;
    UpgradeSet.UpgradeContainer speedUpgrade3;

    UpgradeSet.UpgradeContainer intensityUpgrade1;
    UpgradeSet.UpgradeContainer intensityUpgrade2;
    UpgradeSet.UpgradeContainer intensityUpgrade3;

    UpgradeSet.UpgradeContainer areaUpgrade1;
    UpgradeSet.UpgradeContainer areaUpgrade2;
    UpgradeSet.UpgradeContainer areaUpgrade3;

    UpgradeSet.UpgradeContainer rangeUpgrade1;
    UpgradeSet.UpgradeContainer rangeUpgrade2;
    UpgradeSet.UpgradeContainer rangeUpgrade3;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
        GlobalData.GetInstance().onTutorialTextUpdated += OnTutorialTextUpdated;

        AddUpgrades();
    }

    private void Start()
    {
        GenerateUpgrades();
    }

    private void Update()
    {
        UpdateLightEffect();
    }

    private void BeginLightEffect()
    {
        lightEffectTimer = lightEffectTime;
    }

    private void AddUpgrades()
    {
        speedUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Speed, 0.5f);
        speedUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Speed, 0.75f);
        speedUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Speed, 1.0f);

        intensityUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 0.5f);
        intensityUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 0.75f);
        intensityUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Intensity, 1.0f);

        areaUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 0.5f);
        areaUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 0.75f);
        areaUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Area, 1.0f);

        rangeUpgrade1 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Range, 0.5f);
        rangeUpgrade2 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Range, 0.75f);
        rangeUpgrade3 = new UpgradeSet.UpgradeContainer(UpgradeSet.UpgradeTypes.UP_Range, 1.0f);

        upgradeSet.AddUpgrade(speedUpgrade1);
        upgradeSet.AddUpgrade(speedUpgrade2);
        upgradeSet.AddUpgrade(speedUpgrade3);

        upgradeSet.AddUpgrade(intensityUpgrade1);
        upgradeSet.AddUpgrade(intensityUpgrade2);
        upgradeSet.AddUpgrade(intensityUpgrade3);

        upgradeSet.AddUpgrade(areaUpgrade1);
        upgradeSet.AddUpgrade(areaUpgrade2);
        upgradeSet.AddUpgrade(areaUpgrade3);

        upgradeSet.AddUpgrade(rangeUpgrade1);
        upgradeSet.AddUpgrade(rangeUpgrade2);
        upgradeSet.AddUpgrade(rangeUpgrade3);
    }

    private void GenerateUpgrades()
    {
        //int num upgrades
        int numOptions = upgradeTypeToDisplayName.Count;
        int opt1, opt2;
        opt1 = Random.Range(1, numOptions);
        opt2 = Random.Range(1, numOptions);

        //UpgradeSet.UpgradeTypes opt1Specific, opt2Specific;
        //opt1Specific = (UpgradeSet.UpgradeTypes)opt1;
        //for (int i = 0; i < upgradeSet.GetUpgradesByType(opt1Specific).Count; i++)
        //{
        //    int index = Random.Range(1, upgradeSet.GetUpgradesByType(opt1Specific).Count);
        //    upgrade1 = 
        //}

        //ensure no overlap
        if (opt1 == opt2)
        {
            opt2++;
            if (opt2 >= numOptions) opt2 = 0;
        }

        //init buttons
        upgradeButton1.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt1];
        upgradeButton2.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt2];
        upgrade1 = upgradeSet.GetUpgradeSpecific((UpgradeSet.UpgradeTypes)opt1);// new UpgradeSet.UpgradeContainer((UpgradeSet.UpgradeTypes)opt1, 1.5f);
        upgrade2 = upgradeSet.GetUpgradeSpecific((UpgradeSet.UpgradeTypes)opt2); //new UpgradeSet.UpgradeContainer((UpgradeSet.UpgradeTypes)opt2, 1.5f);

        //activate upgrade panel
        upgradePanel.SetActive(true);
    }

    private void OnLightScoreUpdated(int score, bool isIncrease)
    {
        lightScoreText.text = score.ToString();
        if (isIncrease) BeginLightEffect();
    }

    public void OnSelectUpgrade(int index)
    {
        Debug.LogFormat("Upgrade Option {0} Selected", index);
        switch (index)
        {
            case 0:
                upgradeSet.AddUpgrade(upgrade1);
                upgradeStat(upgrade1);
                break;
            default:
                upgradeSet.AddUpgrade(upgrade2);
                upgradeStat(upgrade2);
                break;
        }
    }

    private void upgradeStat(UpgradeSet.UpgradeContainer upgradeType)
    {
        int typeValue = (int)upgradeType.mType;
        switch (typeValue)
        {
            case 0:
                Debug.Log("NO UPGRADE TYPE SELECTED");
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                lightManager.IncreaseRadiusF(upgradeType.mValue);
                break;
            case 4:
                break;
            default:
                Debug.Log("DEFAULT STATEMENT");
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
