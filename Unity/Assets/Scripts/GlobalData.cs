using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

/*

NOTES:
    - forceRecreate: whether or not a new instance should be made even if one currently exists (false by default, probs shouldn't change)
    - CreateInstance(): creates a new instance of global data (or returns existing unless forced to make a new one)
    - DestroyInstance(): if possible, destroy existing
    - GetInstance(): ALWAYS returns an instance, whether new or existing
*/

public class GlobalData : MonoBehaviour
{
    //STATIC, DATA MANAGEMENT METHODS
    private static GlobalData instance = null;

    public static GlobalData CreateInstance(bool forceRecreate = false)
    {
        //return existing by default
        if (instance != null)
        {
            //if not recreate return
            if (!forceRecreate) return instance;

            Destroy(instance.gameObject);
        }

        //build the new instance
        GameObject go = new GameObject("GLOBAL_DATA");
        instance = go.AddComponent<GlobalData>();
        DontDestroyOnLoad(go);
        return instance;
    }

    public static bool DestroyInstance()
    {
        if (instance == null) return false;

        Destroy(instance.gameObject);
        instance = null;
        return true;
    }

    public static GlobalData GetInstance(bool forceRecreate = false)
    {
        //return existing
        if (instance != null && !forceRecreate) return instance;

        //otherwise make new
        return CreateInstance(forceRecreate);
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    public delegate void OnLightScoreUpdated(int score, bool isIncrease);

    //DATA FUNCTIONS
    public int GetLightScore() { return lightScore; }
    public void DecrementLightScore(int amount)
    {
        lightScore -= amount;
        if (onLightScoreUpdated != null) onLightScoreUpdated.Invoke(lightScore, amount < 0);
    }

    public void IncrementLightScore(int amount)
    {
        lightScore += amount;
        if (onLightScoreUpdated != null) onLightScoreUpdated.Invoke(lightScore, amount >= 0);
    }

    public void SetLightScore(int score)
    {
        if (onLightScoreUpdated != null) onLightScoreUpdated.Invoke(score, score > lightScore);
        lightScore = score;        
    }

    //DATA VARS
    public OnLightScoreUpdated onLightScoreUpdated;

    [SerializeField] private int lightScore = 0;
}
