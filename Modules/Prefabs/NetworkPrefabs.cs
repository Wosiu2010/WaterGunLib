using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WaterGunLib.Modules.Prefabs
{
    public class NetworkPrefabs
    {
        public static void RegisterNetworkPrefab(GameObject prefab)
        {
            if (Plugin.prefabsToNetwork.Contains(prefab)) return;

            Plugin.prefabsToNetwork.Add(prefab);
        }
    }
}
