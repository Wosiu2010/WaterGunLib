using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using BepInEx;
using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using HarmonyLib.Tools;
using Unity.Netcode;
using UnityEngine;
using WaterGunLib.Bundles;
using WaterGunLib.Modules;
using static WaterGunLib.Modules.Enemies;
using static WaterGunLib.Modules.Items;
using static WaterGunLib.Modules.SpawnableOutsideObjects;
using static WaterGunLib.Modules.Unlockables;

namespace WaterGunLib
{
    [BepInPlugin(GUID,NAME,VER)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "WaterGun.WaterGunLib";
        const string VER = "1.3.2";
        const string NAME = "WaterGunLib";


        public static List<GameObject> prefabsToNetwork = new List<GameObject>();

        void Awake()
        {
            

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            string pluginsPath = Paths.PluginPath;

            string[] bundleFiles = Directory.GetFiles(
                pluginsPath,
                "*.watergunlib",
                SearchOption.AllDirectories
            );

            foreach (string file in bundleFiles)
            {
                Debug.Log($"[WaterGunLib]: Found bundle: {file}");

                AssetBundle bundle = AssetBundle.LoadFromFile(file);

                if (bundle == null)
                {
                    Debug.LogError($"[WaterGunLib]: Failed to load bundle: {file}");
                    continue;
                }

                var mods = bundle.LoadAllAssets<WaterGunLibMod>();

                foreach (var mod in mods)
                {
                    foreach (var unlockable in mod.Unlockables)
                    {
                        RegisterUnlockable(unlockable, mod.ModGUID);
                    }
                    foreach (var outsideobject in mod.OutsideObjects)
                    {
                        RegisterSpawnableOutsideObject(outsideobject, mod.ModGUID);
                    }
                    foreach (var enemy in mod.Enemies)
                    {
                        RegisterEnemy(enemy, mod.ModGUID);
                    }
                    foreach (var item in mod.Items)
                    {
                        RegisterItem(item, mod.ModGUID);
                    }
                }
            }



            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();

            Logger.LogInfo("Loaded with all modules");
        }

        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        internal class GameNetworkManager_Start
        {
            [HarmonyPrefix]
            public static void Prefix(GameNetworkManager __instance)
            {
                foreach (GameObject prefab in prefabsToNetwork)
                {
                    if (NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(prefab)) return;

                    NetworkManager.Singleton.AddNetworkPrefab(prefab);
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        internal class StartOfRound_Awake
        {
            [HarmonyPostfix]
            public static void Postfix()
            {

                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {
                    foreach (var pair in registeredEnemies)
                    {
                        RegisteredEnemy key = pair.Key;
                        List<string> value = pair.Value;

                        if (value.Contains("All"))
                        {
                            if (key.spawnType == SpawnType.Indoor)
                            {
                                level.Enemies.Add(new SpawnableEnemyWithRarity
                                {
                                    enemyType = key.enemyType,
                                    rarity = key.rarity
                                });
                            }
                            else if (key.spawnType == SpawnType.Outdoor)
                            {
                                level.OutsideEnemies.Add(new SpawnableEnemyWithRarity
                                {
                                    enemyType = key.enemyType,
                                    rarity= key.rarity
                                });
                            }
                            else if (key.spawnType == SpawnType.DayTime)
                            {
                                level.DaytimeEnemies.Add(new SpawnableEnemyWithRarity
                                {
                                    enemyType = key.enemyType,
                                    rarity = key.rarity
                                });
                            }
                        }
                    }

                    foreach (var pair in registeredOutsideObjects)
                    {
                        RegisteredOutsideObject key = pair.Key;
                        List<string> value = pair.Value;

                        if (value.Contains("All"))
                        {
                            List<SpawnableOutsideObjectWithRarity> objectlist = level.spawnableOutsideObjects.ToList();
                            objectlist.Add(new SpawnableOutsideObjectWithRarity
                            {
                                spawnableObject = key.outsideObject,
                                randomAmount = key.randomAmount
                            });
                            level.spawnableOutsideObjects = objectlist.ToArray();
                        }

                        if (value.Contains(level.PlanetName) && !value.Contains("All"))
                        {
                            List<SpawnableOutsideObjectWithRarity> objectlist = level.spawnableOutsideObjects.ToList();
                            objectlist.Add(new SpawnableOutsideObjectWithRarity
                            {
                                spawnableObject = key.outsideObject,
                                randomAmount = key.randomAmount
                            });
                            level.spawnableOutsideObjects = objectlist.ToArray();
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "Start")]
        internal class Terminal_Start
        {
            [HarmonyPrefix]
            public static void Prefix(Terminal __instance)
            {
                var infoKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

                foreach (var pair in registeredEnemies)
                {
                    RegisteredEnemy key = pair.Key;
                    List<string> value = pair.Value;

                    if (key.infoNode == null)
                    {
                        key.infoNode = ScriptableObject.CreateInstance<TerminalNode>();
                        key.infoNode.displayText = $"No information given about {key.enemyType.enemyName}\n\n";
                        key.infoNode.clearPreviousText = true;
                        key.infoNode.creatureName = key.enemyType.enemyName;
                        key.infoNode.maxCharactersToType = 35;
                    }

                    if (__instance.enemyFiles.Any(file => file.creatureName == key.infoNode.creatureName))
                    {
                        Debug.LogError("[WaterGunLib]: " + key.infoNode.creatureName + "Already exists in enemiesList");
                        continue;
                    }

                    var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                    keyword.word = key.infoNode.creatureName.ToLowerInvariant().Replace(" ", "-");
                    keyword.defaultVerb = infoKeyword;

                    var allKeywordsList = __instance.terminalNodes.allKeywords.ToList();

                    if (!allKeywordsList.Any(x => x.word == keyword.word))
                    {
                        allKeywordsList.Add(keyword);
                        __instance.terminalNodes.allKeywords = allKeywordsList.ToArray();
                    }

                    var infoKeywordNounsList = infoKeyword.compatibleNouns.ToList();

                    if (!infoKeywordNounsList.Any(x => x.noun.word == keyword.word))
                    {
                        infoKeywordNounsList.Add(new CompatibleNoun
                        {
                            noun = keyword,
                            result = key.infoNode
                        });

                        infoKeyword.compatibleNouns = infoKeywordNounsList.ToArray();
                    }


                    key.infoNode.creatureFileID = __instance.enemyFiles.Count;

                    __instance.enemyFiles.Add(key.infoNode);

                    var ScanNodesInEnemy = key.enemyType.enemyPrefab.GetComponentsInChildren<ScanNodeProperties>();

                    foreach (var scannode in ScanNodesInEnemy)
                    {
                        scannode.creatureScanID = key.infoNode.creatureFileID;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "Awake")]
        internal class Terminal_Awake
        {
            [HarmonyPrefix]
            public static void Prefix(Terminal __instance)
            {
                foreach (var unlockable in Unlockables.registeredUnlockables)
                {
                    if (StartOfRound.Instance.unlockablesList.unlockables.Any((UnlockableItem x) => x.unlockableName == unlockable.unlockableItem.unlockableName))
                    {
                        Debug.LogError("[WaterGunLib]: " + unlockable.unlockableItem.unlockableName + "Already exists in unlockablesList");
                        continue;
                    }

                    if (unlockable.unlockableItem.prefabObject != null)
                    {
                        PlaceableShipObject shipObject = unlockable.unlockableItem.prefabObject.GetComponentInChildren<PlaceableShipObject>();
                        if (shipObject != null)
                        {
                            shipObject.unlockableID = StartOfRound.Instance.unlockablesList.unlockables.Count;
                        }
                    }
                    StartOfRound.Instance.unlockablesList.unlockables.Add(unlockable.unlockableItem);
                }      

                var buyKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
                var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
                var infoKeyword = __instance.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

                
                foreach (var item in registeredItems)
                {
                    if (!StartOfRound.Instance.allItemsList.itemsList.Any((Item x) => x.itemName == item.Key.item.itemName))
                        StartOfRound.Instance.allItemsList.itemsList.Add(item.Key.item);

                    if (!item.Key.isShopItem)
                    {
                        foreach (SelectableLevel level in StartOfRound.Instance.levels)
                        {
                            if (!level.spawnableScrap.Any(scrap => scrap.spawnableItem == item.Key.item))
                            {
                                if (item.Value.Contains("All"))
                                {
                                    level.spawnableScrap.Add(new SpawnableItemWithRarity
                                    {
                                        rarity = item.Key.rarity,
                                        spawnableItem = item.Key.item
                                    });
                                }

                                if (item.Value.Contains(level.PlanetName) && !item.Value.Contains("All"))
                                {
                                    level.spawnableScrap.Add(new SpawnableItemWithRarity
                                    {
                                        rarity = item.Key.rarity,
                                        spawnableItem = item.Key.item
                                    });
                                }
                            }
                        }
                    }

                    else if (item.Key.isShopItem)
                    {
                        if (__instance.buyableItemsList.Any(itemScript => itemScript == item.Key.item))
                        {
                            var BuyableItems = __instance.buyableItemsList.ToList();
                            BuyableItems.Add(item.Key.item);
                            __instance.buyableItemsList = BuyableItems.ToArray();
                        }



                        var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                        keyword.name = item.Key.item.itemName.Replace(" ", "-");
                        keyword.defaultVerb = buyKeyword;
                        keyword.word = item.Key.item.itemName.ToLowerInvariant().Replace(" ", "-");

                        if (__instance.terminalNodes.allKeywords.Any(x => x.word == keyword.word))
                        {
                            Debug.LogError($"[WaterGunLib]: Keyword: {keyword.word} already exists");
                            continue;
                        }

                        var itemlist = __instance.buyableItemsList.ToList();

                        var oldIndex = itemlist.IndexOf(item.Key.item);

                        var newIndex = oldIndex == -1 ? itemlist.Count - 1 : oldIndex;

                        var buyNode2 = ScriptableObject.CreateInstance<TerminalNode>();
                        buyNode2.name = $"{item.Key.item.itemName.Replace(" ", "-")}BuyNode2";
                        buyNode2.displayText = $"Ordered [variableAmount] {item.Key.item.itemName}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                        buyNode2.clearPreviousText = true;
                        buyNode2.maxCharactersToType = 15;

                        buyNode2.buyItemIndex = newIndex;
                        buyNode2.creatureName = item.Key.item.itemName;
                        buyNode2.isConfirmationNode = false;
                        buyNode2.itemCost = item.Key.item.creditsWorth;
                        buyNode2.playSyncedClip = 0;



                        TerminalNode buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                        buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                        buyNode1.name = $"{item.Key.item.itemName.Replace(" ", "-")}BuyNode1";
                        buyNode1.displayText = $"You have requested to order {item.Key.item.itemName}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                        buyNode1.clearPreviousText = true;
                        buyNode1.maxCharactersToType = 35;

                        buyNode1.buyItemIndex = newIndex;
                        buyNode1.creatureName = item.Key.item.itemName;
                        buyNode1.isConfirmationNode = true;
                        buyNode1.overrideOptions = true;
                        buyNode1.itemCost = item.Key.item.creditsWorth;
                        buyNode1.terminalOptions = new CompatibleNoun[2]
                        {
                            new CompatibleNoun()
                            {
                                noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm"),
                                result = buyNode2
                            },
                            new CompatibleNoun()
                            {
                                noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny"),
                                result = cancelPurchaseNode
                            }
                        };

                        var allKeywords = __instance.terminalNodes.allKeywords.ToList();
                        allKeywords.Add(keyword);
                        __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                        var nouns = buyKeyword.compatibleNouns.ToList();
                        nouns.Add(new CompatibleNoun()
                        {
                            noun = keyword,
                            result = buyNode1
                        });
                        buyKeyword.compatibleNouns = nouns.ToArray();

                        var itemInfo = item.Key.infoNode;
                        if (itemInfo == null)
                        {
                            itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                            itemInfo.name = $"{item.Key.item.itemName.Replace(" ", "-")}InfoNode";
                            itemInfo.displayText = $"No information given about {item.Key.item.itemName}\n\n";
                            itemInfo.clearPreviousText = true;
                            itemInfo.maxCharactersToType = 25;
                        }

                        __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                        var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                        itemInfoNouns.Add(new CompatibleNoun()
                        {
                            noun = keyword,
                            result = itemInfo
                        });
                        infoKeyword.compatibleNouns = itemInfoNouns.ToArray();

                    }


                }

                foreach (var item in registeredUnlockables)
                {
                    var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
                    keyword.name = item.unlockableItem.unlockableName.Replace(" ", "-");
                    keyword.defaultVerb = buyKeyword;
                    keyword.word = item.unlockableItem.unlockableName.ToLowerInvariant().Replace(" ", "-");

                    if (__instance.terminalNodes.allKeywords.Any(x => x.word == keyword.word))
                    {
                        Debug.LogError($"[WaterGunLib]: Keyword: {keyword.word} already exists");
                        continue;
                    }
                    var index = StartOfRound.Instance.unlockablesList.unlockables.FindIndex(unlockable => unlockable.unlockableName == item.unlockableItem.unlockableName);

                    var buyNode2 = ScriptableObject.CreateInstance<TerminalNode>();
                    buyNode2.name = $"{item.unlockableItem.unlockableName.Replace(" ", "-")}BuyNode2";
                    buyNode2.displayText = $"Ordered [variableAmount] {item.unlockableItem.unlockableName}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                    buyNode2.clearPreviousText = true;
                    buyNode2.maxCharactersToType = 15;

                    buyNode2.buyItemIndex = -1;
                    buyNode2.shipUnlockableID = index;
                    buyNode2.buyUnlockable = true;
                    buyNode2.creatureName = item.unlockableItem.unlockableName;
                    buyNode2.isConfirmationNode = false;
                    buyNode2.itemCost = item.price;
                    buyNode2.playSyncedClip = 0;



                    TerminalNode buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                    buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                    buyNode1.name = $"{item.unlockableItem.unlockableName.Replace(" ", "-")}BuyNode1";
                    buyNode1.displayText = $"You have requested to order {item.unlockableItem.unlockableName}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                    buyNode1.clearPreviousText = true;
                    buyNode1.maxCharactersToType = 35;

                    buyNode1.buyItemIndex = -1;
                    buyNode1.shipUnlockableID = index;
                    buyNode1.creatureName = item.unlockableItem.unlockableName;
                    buyNode1.isConfirmationNode = true;
                    buyNode1.overrideOptions = true;
                    buyNode1.itemCost = item.price;
                    buyNode1.terminalOptions = new CompatibleNoun[2]
                    {
                        new CompatibleNoun()
                        {
                            noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm"),
                            result = buyNode2
                        },
                        new CompatibleNoun()
                        {
                            noun = __instance.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny"),
                            result = cancelPurchaseNode
                        }
                    };

                    if (item.storeType == StoreType.Decor)
                    {
                        item.unlockableItem.shopSelectionNode = buyNode1;
                    }
                    else
                    {
                        item.unlockableItem.shopSelectionNode = null;
                    }

                    var allKeywords = __instance.terminalNodes.allKeywords.ToList();
                    allKeywords.Add(keyword);
                    __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                    var nouns = buyKeyword.compatibleNouns.ToList();
                    nouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = buyNode1
                    });
                    buyKeyword.compatibleNouns = nouns.ToArray();

                    var itemInfo = item.itemInfo;
                    if (itemInfo == null)
                    {
                        itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                        itemInfo.name = $"{item.unlockableItem.unlockableName.Replace(" ", "-")}InfoNode";
                        itemInfo.displayText = $"No information given about {item.unlockableItem.unlockableName}\n\n";
                        itemInfo.clearPreviousText = true;
                        itemInfo.maxCharactersToType = 25;
                    }

                    __instance.terminalNodes.allKeywords = allKeywords.ToArray();

                    var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                    itemInfoNouns.Add(new CompatibleNoun()
                    {
                        noun = keyword,
                        result = itemInfo
                    });
                    infoKeyword.compatibleNouns = itemInfoNouns.ToArray();

                    
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        internal class Terminal_TextPostProcess
        {
            [HarmonyPrefix]
            public static void Prefix(ref TerminalNode node, ref string modifiedDisplayText, Terminal __instance)
            {
                if (modifiedDisplayText.Contains("[buyableItemsList]") && modifiedDisplayText.Contains("[unlockablesSelectionList]"))
                {
                    var index = modifiedDisplayText.IndexOf(@":");

                    foreach (var unlockable in registeredUnlockables)
                    {
                        if (unlockable.storeType == StoreType.ShipUpgrade)
                        {

                            var unlockableName = unlockable.unlockableItem.unlockableName;
                            var unlockablePrice = unlockable.price;

                            var newLine = $"\n* {unlockableName}    //    Price: ${unlockablePrice}";

                            modifiedDisplayText = modifiedDisplayText.Insert(index + 1, newLine);
                        }
                    }
                }
            }
        }
    }
}
