using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterGunLib.Bundles
{
    [CreateAssetMenu(menuName = "WaterGunLib/Mod")]
    public class WaterGunLibMod : ScriptableObject
    {
        [Header("Settings")]
        public string ModGUID;

        [Space(5f)]

        public List<WaterGunLib.Modules.UnlockableItemRef> Unlockables;
        public List<WaterGunLib.Modules.SpawnableOutsideObjectRef> OutsideObjects;
        public List<WaterGunLib.Modules.EnemyTypeRef> Enemies;
        public List<WaterGunLib.Modules.ItemRef> Items;
    }
}
