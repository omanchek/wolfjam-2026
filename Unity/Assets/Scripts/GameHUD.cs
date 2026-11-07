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

    [Header("Upgrades")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject upgradeButton1;
    [SerializeField] private GameObject upgradeButton2;
    [SerializeField] Dictionary<UpgradeSet.UpgradeTypes, string> upgradeTypeToDisplayName;

    [Header("Debug")]
    [SerializeField] UpgradeSet upgradeSet;
    [SerializeField] RectTransform debugUpgradesTransform;

    //members
    float lightEffectTimer = 0.0f;
    UpgradeSet.UpgradeContainer upgrade1;
    UpgradeSet.UpgradeContainer upgrade2;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
        GlobalData.GetInstance().onTutorialTextUpdated += OnTutorialTextUpdated;
    }

    private void Start()
    {
        GenerateUpgrades();
    }

    private void Update()
    {
        UpdateLightEffect();
        DebugUpgradeDisplay();
    }

    private void BeginLightEffect()
    {
        lightEffectTimer = lightEffectTime;
    }

    public void DebugUpgradeDisplay()
    {
        //size upgrades
        List<UpgradeSet.UpgradeContainer> sizeUpgrades = upgradeSet.GetUpgradesByType(UpgradeSet.UpgradeTypes.UP_Area);
        float sizeScale = 1.0f;
        for (int i = 0; i < sizeUpgrades.Count; i++) sizeScale *= sizeUpgrades[i].mValue;
        debugUpgradesTransform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);

        //speed upgrades
        List<UpgradeSet.UpgradeContainer> speedUpgrades = upgradeSet.GetUpgradesByType(UpgradeSet.UpgradeTypes.UP_Speed);
        float speedScale = 1.0f;
        for (int i = 0; i < speedUpgrades.Count; i++) speedScale *= speedUpgrades[i].mValue;
        debugUpgradesTransform.position = new Vector2(960 + 50.0f * Mathf.Cos(Time.time * speedScale), 540 + 50.0f * Mathf.Sin(Time.time * speedScale));
    }

    private void GenerateUpgrades()
    {
        //int num upgrades
        int numOptions = upgradeTypeToDisplayName.Count;
        int opt1, opt2;
        opt1 = Random.Range(1, numOptions);
        opt2 = Random.Range(1, numOptions);
        
        //ensure no overlap
        if (opt1 == opt2)
        {
            opt2++;
            if (opt2 >= numOptions) opt2 = 0;
        }

        //init buttons
        upgradeButton1.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt1];
        upgradeButton2.GetComponentInChildren<TextMeshProUGUI>().text = upgradeTypeToDisplayName[(UpgradeSet.UpgradeTypes)opt2];
        upgrade1 = new UpgradeSet.UpgradeContainer((UpgradeSet.UpgradeTypes)opt1, 1.5f);
        upgrade2 = new UpgradeSet.UpgradeContainer((UpgradeSet.UpgradeTypes)opt2, 1.5f);

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
                break;
            default:
                upgradeSet.AddUpgrade(upgrade2);
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
