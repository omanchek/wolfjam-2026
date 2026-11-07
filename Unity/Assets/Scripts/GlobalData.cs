using UnityEngine;

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

    //DATA FUNCTIONS
    public float GetTestFloat() { return testFloat; }
    public void SetTestFloat(float val) { testFloat = val; }

    //DATA VARS
    [SerializeField] private float testFloat = 0.0f;
}
