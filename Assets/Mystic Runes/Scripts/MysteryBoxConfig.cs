using MusicalRunes;
using System;
using UnityEngine;

namespace MysteryBox
{
    [CreateAssetMenu(fileName = "new MysteryBox Config", menuName = "Configs/MysteryBox", order = 0)]
    public class MysteryBoxConfig : ScriptableObject
    {
        public MysteryBoxType mysteryType;
        public string mysteryNameID;
        public string descriptionID;


        public int[] pricePerLevel = { 50, 100, 200 };
        public int[] cooldownAtlevel = { 5, 4, 3 };
        public bool decreaseCooldownOnRuneActivation;

        public string MysteryTypeName => Localization.GetLocalizedText(mysteryNameID);
        public string Description => Localization.GetLocalizedText(descriptionID);

        public int MaxLevel => pricePerLevel.Length;
        public int GetUpgradePrice(int level) => level >= MaxLevel ? Int32.MaxValue : pricePerLevel[level];
        public int GetCooldown(int level) => level == 0 ? 0 : cooldownAtlevel[level - 1];
    }
}
