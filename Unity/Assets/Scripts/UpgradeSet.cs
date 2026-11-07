using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeSet : MonoBehaviour
{
    public enum UpgradeTypes
    { 
        UP_None = 0,
        UP_Intensity = 1,
        UP_Range = 2,
        UP_Area = 3
    }

    [Serializable]
    public struct UpgradeContainer
    {
        public UpgradeContainer(UpgradeTypes type, float value)
        {
            mType = type;
            mValue = value;
        }

        public UpgradeTypes mType;
        public float mValue;
    }

    //members
    [SerializeField] List<UpgradeContainer> upgradeList;

    public void AddUpgrade(UpgradeContainer upgrade)
    {
        upgradeList.Add(upgrade);
    }

    public List<UpgradeContainer> GetUpgradesByType(UpgradeTypes type)
    {
        List<UpgradeContainer> typeUpgrades = new List<UpgradeContainer>();

        //loop through for matches
        for (int i = 0; i < upgradeList.Count; i++)
        {
            if (upgradeList[i].mType == type) typeUpgrades.Add(upgradeList[i]);
        }

        return typeUpgrades;
    }
}
