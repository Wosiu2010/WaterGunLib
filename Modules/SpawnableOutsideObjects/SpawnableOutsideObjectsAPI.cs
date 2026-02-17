using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static WaterGunLib.Modules.Unlockables;

namespace WaterGunLib.Modules
{
    [CreateAssetMenu(menuName = "WaterGunLib/OutsideObjects/SpawnableOutsideObjectRefrence")]
    public class SpawnableOutsideObjectRef : ScriptableObject
    {
        [Header("Settings")]
        public bool networkPrefab;
        public List<string> PlanetNames;
        public AnimationCurve RandomAmount;
        public SpawnableOutsideObject outsideObject;
    }

    public class SpawnableOutsideObjects
    {

        public static Dictionary<RegisteredOutsideObject, List<string>> registeredOutsideObjects = new Dictionary<RegisteredOutsideObject, List<string>>();

        public class RegisteredOutsideObject
        {
            public string modGUID;
            public SpawnableOutsideObject outsideObject;
            public AnimationCurve randomAmount;
        }

        /// <summary>
        /// Registers SpawnableOutsideObject
        /// </summary>
        public static void RegisterSpawnableOutsideObject(SpawnableOutsideObjectRef outsideObject, string modGUID)
        {
            RegisteredOutsideObject OutsideObject = new RegisteredOutsideObject
            {
                modGUID = modGUID,
                outsideObject = outsideObject.outsideObject,
                randomAmount = outsideObject.RandomAmount,
            };

            if (registeredOutsideObjects.ContainsKey(OutsideObject))
            {
                Debug.LogError($"[WaterGunLib]: {OutsideObject.outsideObject.prefabToSpawn.name} is already registered. modName: {OutsideObject.modGUID}");
            }
            else
            {
                if (outsideObject.networkPrefab) Plugin.prefabsToNetwork.Add(OutsideObject.outsideObject.prefabToSpawn);
                registeredOutsideObjects.Add(OutsideObject, outsideObject.PlanetNames);
                Debug.Log($"[WaterGunLib]: Registered {OutsideObject.outsideObject.prefabToSpawn.name}. modGUID: {OutsideObject.modGUID}");
            }
        }
    }
}
