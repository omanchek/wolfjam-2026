using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int tutorialIndex;
    [SerializeField, Multiline] private string tutorialText;

    [Header("Debug")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private string debugText;    

    private void Awake()
    {
        GlobalData.GetInstance().onTutorialStepUpdated += OnTutorialStepUpdated;
    }

    private void Start()
    {
        //call first step if needed
        if (tutorialIndex == 0) GlobalData.GetInstance().onTutorialStepUpdated.Invoke(-1, this);
    }

    public bool CallTrigger()
    {
        if (!isActive) return false;

        //get the current step
        int currentStep = GlobalData.GetInstance().GetTutorialStep();

        if (currentStep == tutorialIndex)
        {
            GlobalData.GetInstance().SetTutorialStep(tutorialIndex + 1, this);
            if (Application.isEditor) Debug.LogFormat("TUTORIAL STEP: {0}, COMPLETED", debugText);
            isActive = false;
            return true;
        }

        return false;
    }

    public void CallTrigger_Editor()
    {
        CallTrigger();
    }

    public string GetTutorialText()
    {
        return tutorialText;
    }

    private void OnTutorialStepUpdated(int step, TutorialTrigger trigger)
    {
        if (step == tutorialIndex - 1)
        {
            GlobalData.GetInstance().onTutorialTextUpdated.Invoke(tutorialText);
        }
    }
}
