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

    [Header("Debug")]
    [SerializeField] UpgradeSet upgradeSet;
    [SerializeField] RectTransform debugUpgradesTransform;

    //members
    float lightEffectTimer = 0.0f;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
        GlobalData.GetInstance().onTutorialTextUpdated += OnTutorialTextUpdated;
    }

    private void Update()
    {
        UpdateLightEffect();
        //DebugUpgradeDisplay();
    }

    private void BeginLightEffect()
    {
        lightEffectTimer = lightEffectTime;
    }

    public void DebugScoreUpdate(int option)
    {
        switch (option)
        {
            case 0:
                GlobalData.GetInstance().DecrementLightScore(1);
                break;
            case 1:
                GlobalData.GetInstance().IncrementLightScore(1);
                break;
            default:
                GlobalData.GetInstance().SetLightScore(10);
                break;
        }
    }

    public void DebugUpgradeDisplay()
    {
        List<UpgradeSet.UpgradeContainer> sizeUpgrades = upgradeSet.GetUpgradesByType(UpgradeSet.UpgradeTypes.UP_Area);
        float scale = 1.0f;
        for (int i = 0; i < sizeUpgrades.Count; i++) scale *= sizeUpgrades[i].mValue;
        debugUpgradesTransform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnLightScoreUpdated(int score, bool isIncrease)
    {
        lightScoreText.text = score.ToString();
        if (isIncrease) BeginLightEffect();
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
