using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    //components
    [SerializeField] TextMeshProUGUI lightScoreText;

    private void Awake()
    {
        GlobalData.GetInstance().onLightScoreUpdated += OnLightScoreUpdated;
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

    private void OnLightScoreUpdated(int score)
    {
        lightScoreText.text = score.ToString();
    }
}
