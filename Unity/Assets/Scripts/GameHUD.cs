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

    //members
    float lightEffectTimer = 0.0f;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
    }

    private void Update()
    {
        UpdateLightEffect();
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

    private void OnLightScoreUpdated(int score, bool isIncrease)
    {
        lightScoreText.text = score.ToString();
        if (isIncrease) BeginLightEffect();
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
