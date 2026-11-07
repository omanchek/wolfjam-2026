using System;
using System.Collections.Generic;
using UnityEngine;


public class UpgradeSet : MonoBehaviour
{
    public enum UpgradeTypes
    { 
        UP_None = 0, // Does nothing
        UP_Intensity = 1, // Decreases time to scare predator
        UP_Range = 2, // Increases how far the lighthouse can go from the lighthouse
        UP_Area = 3, // Increase area of the light
        UP_Speed = 4 // Increase movement speed
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
    [SerializeField] List<UpgradeContainer> upgradeList; // What the player owns
    [SerializeField] List<UpgradeContainer> upgradeOptions = new List<UpgradeContainer>(); // What can be offered

    public void AddUpgrade(UpgradeContainer upgrade)
    {
        upgradeList.Add(upgrade);
    }

    public void AddUpgradeOption(UpgradeContainer upgrade)
    {
        upgradeOptions.Add(upgrade);
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

    public UpgradeContainer GetUpgradeSpecific(UpgradeTypes type)
    {
        var listOfUpgrades = upgradeOptions.FindAll(upgrade => upgrade.mType == type);
        int index = UnityEngine.Random.Range(0, listOfUpgrades.Count);
        return listOfUpgrades[index];
    }
}
