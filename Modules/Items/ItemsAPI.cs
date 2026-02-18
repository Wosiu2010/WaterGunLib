using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace WaterGunLib.Modules
{
    [CreateAssetMenu(menuName = "WaterGunLib/Items/ItemRefrence")]
    public class ItemRef : ScriptableObject
    {
        [Header("Settings")]
        public bool networkPrefab;
        public Item item;

        [Space(5f)]
        [Header("ShopItem")]
        public TerminalNode infoNode = null;
        public bool isShopItem = false;

        [Space(5f)]
        [Header("Scrap")]
        public int rarity = 10;
        public List<string> PlanetNames = new List<string> {"All"};
    }

    public class Items
    {
        public static Dictionary<RegisteredItem, List<string>> registeredItems = new Dictionary<RegisteredItem, List<string>>();

        public class RegisteredItem
        {
            public string modGUID;
            public Item item;
            public int rarity;
            public TerminalNode infoNode;
            public bool isShopItem;
        }

        /// <summary>
        /// Registers item
        /// </summary>
        public static void RegisterItem(ItemRef itemRef, string modGUID)
        {
            RegisteredItem item = new RegisteredItem
            {
                item = itemRef.item,
                modGUID = modGUID,
                rarity = itemRef.rarity,
                infoNode = itemRef.infoNode,
                isShopItem = itemRef.isShopItem
            };

            if (registeredItems.ContainsKey(item))
            {
                Debug.LogError($"[WaterGunLib]: {item.item.itemName} is already registered. modName: {item.modGUID}");
            }
            else
            {
                if (itemRef.networkPrefab) Plugin.prefabsToNetwork.Add(item.item.spawnPrefab);
                registeredItems.Add(item, itemRef.PlanetNames);
                Debug.Log($"[WaterGunLib]: Registered {item.item.itemName}. modGUID: {item.modGUID}");
            }
        }
    }
}
