using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //configuration
    [Header("References")]
    [SerializeField] RectTransform bgImage;

    [Header("Properties")]
    [SerializeField] Vector3 mainPosition;
    [SerializeField] Vector3 settingsPosition;
    [SerializeField] float transitionLerp = 0.05f;

    //members
    private Vector3 bgGoalPosition;

    private void Awake()
    {
        bgImage.localPosition = mainPosition;
        bgGoalPosition = bgImage.localPosition;
    }

    private void Update()
    {
        bgImage.localPosition = Vector3.Lerp(bgImage.localPosition, bgGoalPosition, transitionLerp);
    }

    public void PanToMain()
    {
        bgGoalPosition = mainPosition;
    }

    public void PanToSettings()
    {
        bgGoalPosition = settingsPosition;
    }
}
