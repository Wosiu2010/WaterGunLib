using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using WaterGunLib.Modules;
using static WaterGunLib.Modules.Unlockables;

#pragma warning disable CS8618
#pragma warning disable CS8625

namespace WaterGunLib.Modules
{

    [CreateAssetMenu(menuName = "WaterGunLib/Unlockables/UnlockableItemRefrence")]
    public class UnlockableItemRef : ScriptableObject
    {
        [Header("Settings")]
        public bool networkPrefab;
        public StoreType StoreType;
        public int Price;
        public TerminalNode InfoNode;
        public UnlockableItem UnlockableItem;
    }

    public class Unlockables
    {
        public static List<RegisteredUnlockable> registeredUnlockables = new List<RegisteredUnlockable>();

        public enum StoreType
        {
            ShipUpgrade,
            Decor
        }

        public class RegisteredUnlockable
        {
            public StoreType storeType;
            public UnlockableItem unlockableItem;
            public string modGUID;
            public int price;
            public TerminalNode itemInfo;
        }


        /// <summary>
        /// Registers a unlockable
        /// </summary>
        public static void RegisterUnlockable(UnlockableItemRef unlockable, string modGUID)
        {
            RegisteredUnlockable furniture = new RegisteredUnlockable
            {
                storeType = unlockable.StoreType,
                unlockableItem = unlockable.UnlockableItem,
                price = unlockable.Price,
                itemInfo = unlockable.InfoNode,
                modGUID = modGUID
            };

            if (registeredUnlockables.Contains(furniture))
            {
                Debug.LogError($"[WaterGunLib]: {unlockable.UnlockableItem.unlockableName} is already registered. modName: {furniture.modGUID}");
            }
            else
            {
                if (unlockable.networkPrefab) Plugin.prefabsToNetwork.Add(unlockable.UnlockableItem.prefabObject);
                registeredUnlockables.Add(furniture);
                Debug.Log($"[WaterGunLib]: Registered {unlockable.UnlockableItem.unlockableName}. modGUID: {furniture.modGUID}");
            }
        }
    }
}
